namespace DotVgn;

/// <summary>
/// Configuration options for the DotVgnClient.
/// </summary>
public record ClientOptions {
    /// <summary>
    /// Gets or sets the base URI used for all API requests.
    /// </summary>
    public Uri BaseEndpoint { get; set; } = new("https://start.vag.de/dm/api/v1/");
}
