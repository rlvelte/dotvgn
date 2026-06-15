using DotVgn.Client.Queries.Base;
using DotVgn.Data.Enumerations;

namespace DotVgn.Client.Queries;

/// <summary>
/// Defines a query to search for trips by type and number.
/// </summary>
public sealed record TripQuery : IQuery {
    /// <summary>
    /// The transport type of the requested trip.
    /// </summary>
    private TransportType TransportType { get; }
    
    /// <summary>
    /// The trip number of the requested trip.
    /// </summary>
    private int TripNumber { get; }
    
    /// <summary>
    /// The date of the requested trip.
    /// </summary>
    private DateTime? Date { get; }

    /// <summary>
    /// Creates a query that searches by trip number and transport type.
    /// </summary>
    /// <param name="transportType">The transport type.</param>
    /// <param name="tripNumber">The trip number.</param>
    public TripQuery(TransportType transportType, int tripNumber) {
        TransportType = transportType;
        TripNumber = tripNumber;
    }

    /// <summary>
    /// Creates a query that searches by trip number, transport type, and date.
    /// </summary>
    /// <param name="transportType">The transport type.</param>
    /// <param name="tripNumber">The trip number.</param>
    /// <param name="date">The date of the trip.</param>
    public TripQuery(TransportType transportType, int tripNumber, DateTime date) : this(transportType, tripNumber) {
        Date = date;
    }
    
    /// <inheritdoc />
    public string GetRelativeUriExtension() {
        return Date != null ?
            $"fahrten/{Uri.EscapeDataString(TransportType.ToString())}/{Date.Value:yyyy-MM-dd}/{TripNumber}" 
            : $"fahrten/{Uri.EscapeDataString(TransportType.ToString())}/{TripNumber}";
    }
}