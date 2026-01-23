using System.Net;
using DotVgn.Client;
using DotVgn.Mapper;
using DotVgn.Mapper.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotVgn;

/// <summary>
/// Extension methods for setting up VAG/VGN client services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions {
    private const string OptionsName = "DotVgn.Client";

    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    extension(IServiceCollection services) {
        /// <summary>
        /// Adds the VGN client using HttpClientFactory, with options configured via callback.
        /// </summary>
        /// <param name="configuration">Delegate to configure <see cref="ClientOptions"/>. Can use the service provider.</param>
        /// <returns>The service collection.</returns>
        public IServiceCollection AddDotVgnClient(Action<ClientOptions> configuration) {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            var options = new ClientOptions();
            configuration(options);

            services.Configure(OptionsName, configuration);
            services.AddOptions<ClientOptions>(OptionsName)
                    .Validate(o => o.BaseEndpoint.IsAbsoluteUri, "DotBahn: BaseEndpoint must be an absolute URI.")
                    .ValidateOnStart();

            services.AddHttpClient<VgnClient>((sp, http) => {
                var opt = sp.GetRequiredService<IOptionsSnapshot<ClientOptions>>().Get(OptionsName);
                http.BaseAddress = opt.BaseEndpoint;
                http.DefaultRequestHeaders.UserAgent.ParseAdd("DotVgn/1.0 (+https://github.com/rlvelte/dotvgn)");
            }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            
            services.AddSingleton<IDepartureMapper, DepartureMapper>();
            services.AddSingleton<IStationMapper, StationMapper>();
            services.AddSingleton<ITripMapper, TripMapper>();
        
            return services;
        }
    }
}