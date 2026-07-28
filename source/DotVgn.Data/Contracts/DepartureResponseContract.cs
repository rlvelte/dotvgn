using System.Text.Json.Serialization;

namespace DotVgn.Data.Contracts;

/// <summary>
/// The departure response that the VAG interface sends back to the client.
/// </summary>
public record DepartureResponseContract {
    /// <summary>
    /// Information about metadata of the response.
    /// </summary>
    [JsonPropertyName("Metadata")]
    public required MetadataContract Metadata { get; init; }

    /// <summary>
    /// All departures that are responses for the query.
    /// </summary>
    [JsonPropertyName("Abfahrten")]
    public IReadOnlyList<DepartureContract> Departures { get; init; } = [];

    /// <summary>
    /// A single departure response entry.
    /// </summary>
    public record DepartureContract {
        /// <summary>
        /// Line name of the departure.
        /// </summary>
        [JsonPropertyName("Linienname")] public string Line { get; init; } = string.Empty;
        /// <summary>
        /// Stop point identifier.
        /// </summary>
        [JsonPropertyName("Haltepunkt")] public string StopPoint { get; init; } = string.Empty;
        /// <summary>
        /// Direction of travel.
        /// </summary>
        [JsonPropertyName("Richtung")] public string Direction { get; init; } = string.Empty;
        /// <summary>
        /// Human-readable direction description.
        /// </summary>
        [JsonPropertyName("Richtungstext")] public string DirectionDescription { get; init; } = string.Empty;
        /// <summary>
        /// Scheduled departure time.
        /// </summary>
        [JsonPropertyName("AbfahrtszeitSoll")] public DateTimeOffset DepartureTimePlanned { get; init; }
        /// <summary>
        /// Actual departure time (real-time).
        /// </summary>
        [JsonPropertyName("AbfahrtszeitIst")] public DateTimeOffset DepartureTimeActual { get; init; }
        /// <summary>
        /// Transport product type.
        /// </summary>
        [JsonPropertyName("Produkt")] public string Transport { get; init; } = string.Empty;
        /// <summary>
        /// Trip number identifier.
        /// </summary>
        [JsonPropertyName("Fahrtnummer")] public int TripNumber { get; init; }
        /// <summary>
        /// Operating day of the trip.
        /// </summary>
        [JsonPropertyName("Betriebstag")] public DateTimeOffset Date { get; init; }
        /// <summary>
        /// Current occupancy level.
        /// </summary>
        [JsonPropertyName("Besetzgrad")] public string OccupationLevel { get; init; } = string.Empty;
    }
}
