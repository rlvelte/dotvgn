using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace DotVgn.Client.Additional;

/// <summary>
/// Configuration options for the DotVgnClient.
/// </summary>
public sealed class DotVgnClientOptions : IOptions<DotVgnClientOptions> {
    /// <summary>
    /// Gets or sets the base URI used for all API requests.
    /// </summary>
    public Uri BaseUri { get; set; } = new("https://start.vag.de/dm/api/v1/");

    /// <summary>
    /// Gets or sets serializer settings used for JSON serialization.
    /// </summary>
    public JsonSerializerOptions Json { get; set; } = new() {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// Time to keep station query results cached in memory. Set to TimeSpan.Zero or negative to disable caching.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan StationCacheTtl { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of parallel HTTP requests used by batch operations
    /// such as GetDeparturesForStationsAsync. Values less than 1 will be coerced to 1.
    /// Default is 4.
    /// </summary>
    public int MaxParallelRequestsForBatch { get; set; } = 4;

    /// <summary>
    /// Returns this instance as the options value.
    /// </summary>
    public DotVgnClientOptions Value => this;
}
