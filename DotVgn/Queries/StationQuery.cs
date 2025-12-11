using System.Globalization;
using DotVgn.Queries.Base;

namespace DotVgn.Queries;

/// <summary>
/// Defines a query to search for stations by name or by geo-coordinates.
/// </summary>
public sealed record StationQuery : IQuery {
    /// <summary>
    /// The station name used for name-based searching.
    /// </summary>
    private string? Name { get; }

    /// <summary>
    /// The latitude used for geo-based searching.
    /// </summary>
    private double? Latitude { get; }

    /// <summary>
    /// The longitude used for geo-based searching.
    /// </summary>
    private double? Longitude { get; }

    /// <summary>
    /// The radius used for geo-based searching.
    /// </summary>
    private int? Radius { get; }

    /// <summary>
    /// Creates a query that searches by station name only.
    /// </summary>
    /// <param name="name">The station name.</param>
    public StationQuery(string name) {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Creates a query that searches by geo-coordinates.
    /// </summary>
    /// <param name="latitude">The latitude.</param>
    /// <param name="longitude">The longitude.</param>
    /// <param name="radius">The search radius.</param>
    public StationQuery(double latitude, double longitude, int radius) {
        Latitude = latitude;
        Longitude = longitude;
        Radius = radius;
    }

    /// <inheritdoc />
    public string GetRelativeUriExtension() {
        if (Name != null) {
            return $"haltestellen/vgn?name={Uri.EscapeDataString(Name)}";
        }

        if (Latitude.HasValue && Longitude.HasValue && Radius.HasValue) {
            return $"haltestellen/vgn/location?lon={Longitude.Value.ToString(CultureInfo.InvariantCulture)}" +
                   $"&lat={Latitude.Value.ToString(CultureInfo.InvariantCulture)}" +
                   $"&radius={Radius.Value}";
        }

        throw new InvalidOperationException("StationQuery must contain either a 'Name' or valid 'Latitude' and 'Longitude'.");
    }
}