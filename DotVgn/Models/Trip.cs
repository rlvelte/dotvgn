namespace DotVgn.Models;

/// <summary>
/// Represents a trip between two stations.
/// </summary>
/// <param name="Line">The line of the trip.</param>
/// <param name="Direction">The direction of the trip.</param>
/// <param name="DirectionDescription">The description of the direction.</param>
/// <param name="Stops">All stops on this trip.</param>
public record Trip(string Line, string Direction, string DirectionDescription, IEnumerable<TripStop> Stops);

/// <summary>
/// Represents a stop on a trip.
/// </summary>
/// <param name="StationName">The station name.</param>
/// <param name="StationId">The station id.</param>
/// <param name="Platform">The platform name.</param>
/// <param name="Latitude">The latitude of the station</param>
/// <param name="Longitude">The longitude of the station</param>
/// <param name="ArrivalTimeEstimated">The estimated arrival time.</param>
/// <param name="ArrivalTimeActual">The actual arrival time.</param>
/// <param name="DepartureTimeEstimated">The estimated departure time.</param>
/// <param name="DepartureTimeActual">The actual departure time.</param>
public record TripStop(string StationName, int StationId, string Platform, double Latitude, double Longitude, DateTime? ArrivalTimeEstimated, DateTime? ArrivalTimeActual, DateTime? DepartureTimeEstimated, DateTime? DepartureTimeActual);