using System.Net;
using DotVgn.Client;
using DotVgn.Client.Additional;
using DotVgn.Client.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace DotVgn;

/// <summary>
/// Extension methods to register DotVGN client with DI.
/// </summary>
public static class ServiceCollectionExtensions {
    private const string DefaultSectionName = "DotVgn";
    private const string UserAgentHeader = "DotVgn/1.0 (+https://github.com/rlvelte/DotVgn)";
    private const string AcceptHeader = "application/json";

    /// <param name="services">The service collection.</param>
    extension(IServiceCollection services) {
        /// <summary>
        /// Adds the DotVGN client using HttpClientFactory, with options configured via callback.
        /// </summary>
        /// <param name="configure">Delegate to configure <see cref="DotVgnClientOptions"/>. Can use the service provider.</param>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddDotVgnClient(Action<IServiceProvider, DotVgnClientOptions> configure) {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            ConfigureOptions(services, sp => opt => configure(sp, opt));
            RegisterHttpClient(services);
        
            return services;
        }

        /// <summary>
        /// Adds the DotVGN client using HttpClientFactory, binding options from the configuration section.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="sectionName">Optional section name. Defaults to "DotVgn".</param>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddDotVgnClient(IConfiguration configuration, string sectionName = DefaultSectionName) {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var section = configuration.GetSection(sectionName);
            ConfigureOptions(services, _ => section.Bind);
            RegisterHttpClient(services);
        
            return services;
        }

        /// <summary>
        /// Adds the DotVGN client using HttpClientFactory, with options configured via simple callback.
        /// </summary>
        /// <param name="configure">Delegate to configure <see cref="DotVgnClientOptions"/>.</param>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddDotVgnClient(Action<DotVgnClientOptions> configure) {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);

            services.AddOptions<DotVgnClientOptions>()
                    .Configure(configure)
                    .ValidateOptions();

            RegisterHttpClient(services);
        
            return services;
        }
    }

    private static void ConfigureOptions(IServiceCollection services, Func<IServiceProvider, Action<DotVgnClientOptions>> configureFactory) {
        services.AddSingleton<IConfigureOptions<DotVgnClientOptions>>(
            sp => new ConfigureOptions<DotVgnClientOptions>(configureFactory(sp)));
        
        services.AddOptions<DotVgnClientOptions>()
            .ValidateOptions();
    }

    private static OptionsBuilder<DotVgnClientOptions> ValidateOptions(this OptionsBuilder<DotVgnClientOptions> builder) {
        return builder
            .Validate(o => o.BaseUri.IsAbsoluteUri, 
                "DotVgn: BaseUri must be an absolute URI.")
            .Validate(o => o.MaxParallelRequestsForBatch >= 1, 
                "DotVgn: MaxParallelRequestsForBatch must be >= 1.")
            .ValidateOnStart();
    }

    private static void RegisterHttpClient(IServiceCollection services) {
        services.AddHttpClient<DotVgnClient>(ConfigureHttpClient)
            .ConfigurePrimaryHttpMessageHandler(CreateHttpMessageHandler)
            .AddPolicyHandler((_, _) => ResiliencePolicyFactory.BuildPolicy());

        services.AddTransient<IDotVgnClient>(sp => sp.GetRequiredService<DotVgnClient>());
    }

    private static void ConfigureHttpClient(IServiceProvider sp, HttpClient http) {
        var options = sp.GetRequiredService<IOptions<DotVgnClientOptions>>().Value;
        
        http.BaseAddress = options.BaseUri;
        http.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentHeader);
        http.DefaultRequestHeaders.Accept.ParseAdd(AcceptHeader);
    }

    private static HttpMessageHandler CreateHttpMessageHandler() {
        return new SocketsHttpHandler {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
    }
}

/// <summary>
/// Factory for creating resilience policies for HTTP requests.
/// </summary>
internal static class ResiliencePolicyFactory {
    private const int TimeoutSeconds = 10;
    private const int RetryCount = 4;
    private const int MaxRetryAfterSeconds = 30;
    private const int MaxBackoffSeconds = 20;
    private const int MaxJitterMilliseconds = 250;
    private const int CircuitBreakerThreshold = 5;
    private const int CircuitBreakerDurationSeconds = 30;

    public static IAsyncPolicy<HttpResponseMessage> BuildPolicy() {
        var timeout = CreateTimeoutPolicy();
        var retry = CreateRetryPolicy();
        var circuitBreaker = CreateCircuitBreakerPolicy();

        return Policy.WrapAsync(retry, circuitBreaker, timeout);
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateTimeoutPolicy() {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(TimeoutSeconds));
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy() {
        var jitter = new Random();

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode is (HttpStatusCode)429 or HttpStatusCode.ServiceUnavailable)
            .WaitAndRetryAsync(
                retryCount: RetryCount,
                sleepDurationProvider: (retryAttempt, outcome, _) => 
                    CalculateRetryDelay(retryAttempt, outcome, jitter),
                onRetryAsync: (_, _, _, _) => Task.CompletedTask
            );
    }

    private static TimeSpan CalculateRetryDelay(int retryAttempt, DelegateResult<HttpResponseMessage> outcome, Random jitter) {
        var retryAfterDelay = TryGetRetryAfterDelay(outcome?.Result);
        if (retryAfterDelay.HasValue)
        {
            return retryAfterDelay.Value;
        }

        var backoff = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
        var jitterMs = jitter.Next(0, MaxJitterMilliseconds);
        var delay = backoff + TimeSpan.FromMilliseconds(jitterMs);

        return delay > TimeSpan.FromSeconds(MaxBackoffSeconds) 
            ? TimeSpan.FromSeconds(MaxBackoffSeconds) 
            : delay;
    }

    private static TimeSpan? TryGetRetryAfterDelay(HttpResponseMessage? response) {
        var retryAfter = response?.Headers?.RetryAfter;
        if (retryAfter == null)
        {
            return null;
        }

        var delay = retryAfter.Delta ?? 
            (retryAfter.Date.HasValue 
                ? retryAfter.Date.Value - DateTimeOffset.UtcNow 
                : null);

        if (!delay.HasValue || delay.Value <= TimeSpan.Zero)
        {
            return null;
        }

        var cappedDelay = delay.Value > TimeSpan.FromSeconds(MaxRetryAfterSeconds)
            ? TimeSpan.FromSeconds(MaxRetryAfterSeconds)
            : delay.Value;

        return cappedDelay;
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy() {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(r => r.StatusCode == (HttpStatusCode)429)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: CircuitBreakerThreshold,
                durationOfBreak: TimeSpan.FromSeconds(CircuitBreakerDurationSeconds)
            );
    }
}