using System.Text.Json.Serialization;

namespace DotVgn.Models.API.Additional;

/// <summary>
/// The metadata that the VAG interface sends back to the client.
/// </summary>
internal record ApiMetadata {
    [JsonPropertyName("Version")] public required string Version { get; init; }
    [JsonPropertyName("Timestamp")] public required string Timestamp { get; init; }
}