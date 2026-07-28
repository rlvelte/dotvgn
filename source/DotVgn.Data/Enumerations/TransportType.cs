namespace DotVgn.Data.Enumerations;

/// <summary>
/// All available transport types within the VGN.
/// </summary>
public enum TransportType {
    /// <summary>
    /// Bus transport.
    /// </summary>
    Bus,
    /// <summary>
    /// Tram transport.
    /// </summary>
    Tram,
    /// <summary>
    /// U-Bahn (subway) transport.
    /// </summary>
    UBahn,
    /// <summary>
    /// S-Bahn (suburban rail) transport.
    /// </summary>
    SBahn,
    /// <summary>
    /// Regional train transport.
    /// </summary>
    RBahn,
    /// <summary>
    /// Unknown or unsupported transport type.
    /// </summary>
    Unknown
}
