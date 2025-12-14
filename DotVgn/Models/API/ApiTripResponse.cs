using System.Text.Json.Serialization;
using DotVgn.Models.API.Additional;

namespace DotVgn.Models.API;

/// <summary>
/// The trip response that the VAG interface sends back to the client.
/// </summary>
internal record ApiTripResponse {
    /// <summary>
    /// Information about metadata of the response.
    /// </summary>
    [JsonPropertyName("Metadata")]
    public required ApiMetadata Metadata { get; init; }
    
    /// <summary>
    /// The direction of the trip.
    /// </summary>
    [JsonPropertyName("Linienname")] 
    public required string Line { get; init; }
    
    /// <summary>
    /// The direction of the trip.
    /// </summary>
    [JsonPropertyName("Richtung")] 
    public required string Direction { get; init; }
    
    /// <summary>
    /// The direction description.
    /// </summary>
    [JsonPropertyName("Richtungstext")] 
    public required string DirectionDescription { get; init; }

    /// <summary>
    /// All stops that are on this trip.
    /// </summary>
    [JsonPropertyName("Fahrtverlauf")]
    public IEnumerable<StopResponse> Stops { get; init; }
    
    /// <summary>
    /// A single stop response entry.
    /// </summary>
    public record StopResponse {
        [JsonPropertyName("Haltestellenname")] public required string Name { get; init; }
        [JsonPropertyName("VAGKennung")] public required string VagId { get; init; }
        [JsonPropertyName("VGNKennung")] public required int VgnId { get; init; }
        [JsonPropertyName("Haltepunkt")] public required string Platform { get; init; }
        [JsonPropertyName("AnkunftszeitSoll")] public DateTime? ArrivalTimeEstimated { get; init; }
        [JsonPropertyName("AnkunftszeitIst")] public DateTime? ArrivalTimeActual { get; init; }
        [JsonPropertyName("AbfahrtszeitSoll")] public DateTime? DepartureTimeEstimated { get; init; }
        [JsonPropertyName("AbfahrtszeitIst")] public DateTime? DepartureTimeActual { get; init; }
        [JsonPropertyName("Latitude")] public required double Latitude { get; init; }
        [JsonPropertyName("Longitude")] public required double Longitude { get; init; }
    }
}