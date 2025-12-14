using System.Net;
using DotVgn.Client;
using DotVgn.Client.Additional;
using DotVgn.Client.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotVgn;

/// <summary>
/// Extension methods to register DotVGN client with DI.
/// </summary>
public static class ServiceCollectionExtensions {
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

            services.AddSingleton<IConfigureOptions<DotVgnClientOptions>>(sp => new ConfigureOptions<DotVgnClientOptions>(opt => configure(sp, opt)));
            services.AddOptions<DotVgnClientOptions>()
                    .Validate(o => o.BaseUri.IsAbsoluteUri, "DotVgn: BaseUri must be an absolute URI.")
                    .Validate(o => o.MaxParallelRequestsForBatch >= 1, "DotVgn: MaxParallelRequestsForBatch must be >= 1.")
                    .ValidateOnStart();
            
            services.AddHttpClient<DotVgnClient>((sp, http) => { 
                        var options = sp.GetRequiredService<IOptions<DotVgnClientOptions>>().Value;
                        http.BaseAddress = options.BaseUri;
                        http.DefaultRequestHeaders.UserAgent.ParseAdd("DotVgn/1.0 (+https://github.com/rlvelte/DotVgn)");
                        http.DefaultRequestHeaders.Accept.ParseAdd("application/json"); 
                    }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    });
            
            services.AddTransient<IDotVgnClient>(sp => sp.GetRequiredService<DotVgnClient>());
            return services;
        }

        /// <summary>
        /// Adds the DotVGN client using HttpClientFactory, binding options from the configuration section.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="sectionName">Optional section name. Defaults to "DotVgn".</param>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddDotVgnClient(IConfiguration configuration, string sectionName = "DotVgn") {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var section = configuration.GetSection(sectionName);

            services.AddSingleton<IConfigureOptions<DotVgnClientOptions>>(_ => new ConfigureOptions<DotVgnClientOptions>(opt => section.Bind(opt)));
            services.AddOptions<DotVgnClientOptions>()
                    .Validate(o => o.BaseUri.IsAbsoluteUri, "DotVgn: BaseUri must be an absolute URI.")
                    .Validate(o => o.MaxParallelRequestsForBatch >= 1, "DotVgn: MaxParallelRequestsForBatch must be >= 1.")
                    .ValidateOnStart();
            
            services.AddHttpClient<DotVgnClient>((sp, http) => {
                        var options = sp.GetRequiredService<IOptions<DotVgnClientOptions>>().Value;
                        http.BaseAddress = options.BaseUri;
                        http.DefaultRequestHeaders.UserAgent.ParseAdd("DotVgn/1.0 (+https://github.com/rlvelte/DotVgn)");
                        http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                    }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    });

            services.AddTransient<IDotVgnClient>(sp => sp.GetRequiredService<DotVgnClient>());
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
                    .Validate(o => o.BaseUri.IsAbsoluteUri, "DotVgn: BaseUri must be an absolute URI.")
                    .Validate(o => o.MaxParallelRequestsForBatch >= 1, "DotVgn: MaxParallelRequestsForBatch must be >= 1.")
                    .ValidateOnStart();

            services.AddHttpClient<DotVgnClient>((sp, http) => {
                        var options = sp.GetRequiredService<IOptions<DotVgnClientOptions>>().Value;
                        http.BaseAddress = options.BaseUri;
                        http.DefaultRequestHeaders.UserAgent.ParseAdd("DotVgn/1.0 (+https://github.com/rlvelte/DotVgn)");
                        http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                    }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                    });

            services.AddTransient<IDotVgnClient>(sp => sp.GetRequiredService<DotVgnClient>());
            return services;
        }
    }
}
