using System.Text.Json.Serialization;

namespace DotVgn.Data.Contracts;

/// <summary>
/// The station response that the VAG interface sends back to the client.
/// </summary>
public record StationResponseContract {
    /// <summary>
    /// Information about metadata of the response.
    /// </summary>
    [JsonPropertyName("Metadata")]
    public required MetadataContract Metadata { get; init; }

    /// <summary>
    /// All stations that are responses for the query.
    /// </summary>
    [JsonPropertyName("Haltestellen")]
    public IEnumerable<StationContract> Stations { get; init; } = [];

    /// <summary>
    /// A single station response entry.
    /// </summary>
    public record StationContract {
        [JsonPropertyName("Haltestellenname")] public string Name { get; init; } = string.Empty;
        [JsonPropertyName("VAGKennung")] public string VagId { get; init; } = string.Empty;
        [JsonPropertyName("VGNKennung")] public int VgnId { get; init; }
        [JsonPropertyName("Latitude")] public double Latitude { get; init; }
        [JsonPropertyName("Longitude")] public double Longitude { get; init; }
        [JsonPropertyName("Produkte")] public string Transports { get; init; } = string.Empty;
    }
}