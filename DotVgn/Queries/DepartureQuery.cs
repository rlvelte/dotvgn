using DotVgn.Models.Enumerations;
using DotVgn.Queries.Base;

namespace DotVgn.Queries;

/// <summary>
/// Defines a query to search for departures by line or by transport filters.
/// </summary>
public sealed record DepartureQuery : IQuery {
    /// <summary>
    /// The station id for which departures are queried.
    /// </summary>
    private int StationId { get; }

    /// <summary>
    /// Optional line filter. If set, transports must be empty.
    /// </summary>
    private string? Line { get; }

    /// <summary>
    /// Optional transport filters. If set, line must be null.
    /// </summary>
    private TransportType[]? Transports { get; }

    /// <summary>
    /// Maximum number of returned departures.
    /// </summary>
    private int Limit { get; }

    /// <summary>
    /// Time window for the query in minutes.
    /// </summary>
    private int Timespan { get; }

    /// <summary>
    /// Time shift for the request in minutes.
    /// </summary>
    private int Delay { get; }

    /// <summary>
    /// Creates a query that searches by line.
    /// </summary>
    /// <param name="stationId">The unique identifier of the station for which departures are queried.</param>
    /// <param name="line">The line to search for.</param>
    /// <param name="limit">The maximum number of departures to return.</param>
    /// <param name="timespan">The time window, in minutes, within which to search for departures. Defaults to 10 minutes if not specified.</param>
    /// <param name="delay">The minimum delay, in minutes, to apply before considering departures. Defaults to 5 minutes if not specified.</param>
    public DepartureQuery(int stationId, string line, int limit, int timespan = 10, int delay = 5) {
        StationId = stationId;
        Line = line;
        Limit = limit;
        Timespan = timespan;
        Delay = delay;
        Transports = null;
    }

    /// <summary>
    /// Creates a query that searches by transport types.
    /// </summary>
    /// <param name="stationId">The unique identifier of the station for which departures are queried.</param>
    /// <param name="transports">An array of transport types to include in the query. Must not be empty.</param>
    /// <param name="limit">The maximum number of departures to return.</param>
    /// <param name="timespan">The time window, in minutes, within which to search for departures. Defaults to 10 minutes if not specified.</param>
    /// <param name="delay">The minimum delay, in minutes, to apply before considering departures. Defaults to 5 minutes if not specified.</param>
    /// <exception cref="ArgumentException">Thrown if transports is an empty array.</exception>
    public DepartureQuery(int stationId, TransportType[] transports, int limit, int timespan = 10, int delay = 5) {
        if (transports.Length == 0) {
            throw new ArgumentException("Transports must not be null or empty.", nameof(transports));
        }

        StationId = stationId;
        Transports = transports;
        Limit = limit;
        Timespan = timespan;
        Delay = delay;
        Line = null;
    }

    /// <summary>
    /// Converts the transports array to a comma-separated string.
    /// </summary>
    private string ParseTransports() => string.Join(",", Transports!);

    /// <inheritdoc />
    public string GetRelativeUriExtension() {
        if (Line != null) {
            return $"abfahrten/vgn/{StationId}/{Line}?timespan={Timespan}&timedelay={Delay}&limitcount={Limit}";
        }

        if (Transports != null && Transports.Any()) {
            return $"abfahrten/vgn/{StationId}?product={ParseTransports()}&timespan={Timespan}&timedelay={Delay}&limitcount={Limit}";
        }

        throw new InvalidOperationException("DepartureQuery must contain either a 'Line' or 'Transports'.");
    }
}
