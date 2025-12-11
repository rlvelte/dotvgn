using System.Text.Json.Serialization;

namespace DotVgn.Models.API;

/// <summary>
/// The response that the VAG interface sends back to the client. Contains only primitive data types.
/// </summary>
public record ApiStationResponse {
    /// <summary>
    /// Information about metadata of the response.
    /// </summary>
    [JsonPropertyName("Metadata")]
    public required ApiMetadataResponse Metadata { get; init; }

    /// <summary>
    /// All stations that are responses for the query.
    /// </summary>
    [JsonPropertyName("Haltestellen")]
    public required IEnumerable<StationResponse> Stations { get; init; }

    /// <summary>
    /// A single station response entry.
    /// </summary>
    public record StationResponse {
        [JsonPropertyName("Haltestellenname")] public required string Name { get; init; }
        [JsonPropertyName("VAGKennung")] public required string VagId { get; init; }
        [JsonPropertyName("VGNKennung")] public required int VgnId { get; init; }
        [JsonPropertyName("Latitude")] public required double Latitude { get; init; }
        [JsonPropertyName("Longitude")] public required double Longitude { get; init; }
        [JsonPropertyName("Produkte")] public required string? Transports { get; init; }
    }
}