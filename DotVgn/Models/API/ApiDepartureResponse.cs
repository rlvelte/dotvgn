using System.Text.Json.Serialization;
using DotVgn.Models.API.Additional;

namespace DotVgn.Models.API;

/// <summary>
/// The departure response that the VAG interface sends back to the client.
/// </summary>
internal record ApiDepartureResponse {
    /// <summary>
    /// Information about metadata of the response.
    /// </summary>
    [JsonPropertyName("Metadata")]
    public required ApiMetadata Metadata { get; init; }

    /// <summary>
    /// All departures that are responses for the query.
    /// </summary>
    [JsonPropertyName("Abfahrten")]
    public required IReadOnlyList<DepartureResponse> Departures { get; init; }

    /// <summary>
    /// A single departure response entry.
    /// </summary>
    public record DepartureResponse {
        [JsonPropertyName("Linienname")] public required string Line { get; init; }
        [JsonPropertyName("Haltepunkt")] public required string StopPoint { get; init; }
        [JsonPropertyName("Richtung")] public required string Direction { get; init; }
        [JsonPropertyName("Richtungstext")] public required string DirectionDescription { get; init; }
        [JsonPropertyName("AbfahrtszeitSoll")] public DateTimeOffset DepartureTimePlanned { get; init; }
        [JsonPropertyName("AbfahrtszeitIst")] public DateTimeOffset DepartureTimeActual { get; init; }
        [JsonPropertyName("Produkt")] public required string Transport { get; init; }
        [JsonPropertyName("Fahrtnummer")] public int TripNumber { get; init; }
        [JsonPropertyName("Betriebstag")] public DateTimeOffset Date { get; init; }
        [JsonPropertyName("Besetzgrad")] public required string OccupationLevel { get; init; }
    }
}