using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace DotVgn.Client;

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
    /// Returns this instance as the options value.
    /// </summary>
    public DotVgnClientOptions Value => this;
}
