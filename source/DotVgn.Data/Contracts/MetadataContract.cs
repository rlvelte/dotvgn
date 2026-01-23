using System.Text.Json.Serialization;

namespace DotVgn.Data.Contracts;

/// <summary>
/// The metadata that the VAG interface sends back to the client.
/// </summary>
public record MetadataContract {
    /// <summary>
    /// Version the api currently is.
    /// </summary>
    [JsonPropertyName("Version")] 
    public required string Version { get; init; }
    
    /// <summary>
    /// Timestamp of the response.
    /// </summary>
    [JsonPropertyName("Timestamp")] 
    public required string Timestamp { get; init; }
}