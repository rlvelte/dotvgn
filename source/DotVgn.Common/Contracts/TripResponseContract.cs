using System.Text.Json.Serialization;

namespace DotVgn.Common.Contracts;

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
        /// <summary>
        /// Name of the stop.
        /// </summary>
        [JsonPropertyName("Haltestellenname")] public string Name { get; init; } = string.Empty;
        /// <summary>
        /// VAG stop identifier.
        /// </summary>
        [JsonPropertyName("VAGKennung")] public string VagId { get; init; } = string.Empty;
        /// <summary>
        /// VGN stop identifier.
        /// </summary>
        [JsonPropertyName("VGNKennung")] public int VgnId { get; init; }
        /// <summary>
        /// Platform identifier at this stop.
        /// </summary>
        [JsonPropertyName("Haltepunkt")] public string Platform { get; init; } = string.Empty;
        /// <summary>
        /// Scheduled arrival time.
        /// </summary>
        [JsonPropertyName("AnkunftszeitSoll")] public DateTime? ArrivalTimeEstimated { get; init; }
        /// <summary>
        /// Actual arrival time (real-time).
        /// </summary>
        [JsonPropertyName("AnkunftszeitIst")] public DateTime? ArrivalTimeActual { get; init; }
        /// <summary>
        /// Scheduled departure time.
        /// </summary>
        [JsonPropertyName("AbfahrtszeitSoll")] public DateTime? DepartureTimeEstimated { get; init; }
        /// <summary>
        /// Actual departure time (real-time).
        /// </summary>
        [JsonPropertyName("AbfahrtszeitIst")] public DateTime? DepartureTimeActual { get; init; }
        /// <summary>
        /// Latitude coordinate of the stop.
        /// </summary>
        [JsonPropertyName("Latitude")] public double Latitude { get; init; }
        /// <summary>
        /// Longitude coordinate of the stop.
        /// </summary>
        [JsonPropertyName("Longitude")] public double Longitude { get; init; }
    }
}
