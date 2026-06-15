using System.Text.Json.Serialization;

namespace DotVgn.Data.Contracts;

/// <summary>
/// The trip response that the VAG interface sends back to the client.
/// </summary>
public record TripResponseContract {
    /// <summary>
    /// Information about metadata of the response.
    /// </summary>
    [JsonPropertyName("Metadata")]
    public required MetadataContract Metadata { get; init; }
    
    /// <summary>
    /// The direction of the trip.
    /// </summary>
    [JsonPropertyName("Linienname")] 
    public string Line { get; init; } = string.Empty;
    
    /// <summary>
    /// The direction of the trip.
    /// </summary>
    [JsonPropertyName("Richtung")] 
    public string Direction { get; init; } = string.Empty;
    
    /// <summary>
    /// The direction description.
    /// </summary>
    [JsonPropertyName("Richtungstext")] 
    public string DirectionDescription { get; init; } = string.Empty;

    /// <summary>
    /// All stops that are on this trip.
    /// </summary>
    [JsonPropertyName("Fahrtverlauf")]
    public IEnumerable<StopContract> Stops { get; init; } = [];
    
    /// <summary>
    /// A single stop response entry.
    /// </summary>
    public record StopContract {
        [JsonPropertyName("Haltestellenname")] public string Name { get; init; } = string.Empty;
        [JsonPropertyName("VAGKennung")] public string VagId { get; init; } = string.Empty;
        [JsonPropertyName("VGNKennung")] public int VgnId { get; init; }
        [JsonPropertyName("Haltepunkt")] public string Platform { get; init; } = string.Empty;
        [JsonPropertyName("AnkunftszeitSoll")] public DateTime? ArrivalTimeEstimated { get; init; }
        [JsonPropertyName("AnkunftszeitIst")] public DateTime? ArrivalTimeActual { get; init; }
        [JsonPropertyName("AbfahrtszeitSoll")] public DateTime? DepartureTimeEstimated { get; init; }
        [JsonPropertyName("AbfahrtszeitIst")] public DateTime? DepartureTimeActual { get; init; }
        [JsonPropertyName("Latitude")] public double Latitude { get; init; }
        [JsonPropertyName("Longitude")] public double Longitude { get; init; }
    }
}