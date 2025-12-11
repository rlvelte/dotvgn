using System.Text.Json.Serialization;

namespace dotVGN.Models.API;

/// <summary>
/// Information about metadata of the response.
/// </summary>
public record ApiMetadataResponse {
    [JsonPropertyName("Version")] public required string Version { get; init; }
    [JsonPropertyName("Timestamp")] public required string Timestamp { get; init; }
}