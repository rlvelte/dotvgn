using DotVgn.Models.Enumerations;

namespace DotVgn.Models;

/// <summary>
/// Represents a specific departure from a station.
/// </summary>
/// <param name="Line">The line on which the vehicle departs.</param>
/// <param name="StopPoint">The point at which the vehicle stops. (e.g., Platform 1)</param>
/// <param name="Direction">The direction the vehicle drives to. (VGN uses "Richtung1", "Richtung2" etc.)</param>
/// <param name="DirectionDescription">The Final destination name.</param>
/// <param name="Date">The date of the departure.</param>
/// <param name="DepartureTimePlanned">Time on which departure was planned.</param>
/// <param name="DepartureTimeActual">Time on which departure is in real life.</param>
/// <param name="TransportType">The transport type.</param>
/// <param name="TripNumber">VGN internal trip number.</param>
/// <param name="OccupationLevel">The level of occupation in the vehicle.</param>
public record Departure(string Line, string StopPoint, string Direction, string DirectionDescription, DateTime Date, DateTime DepartureTimePlanned, DateTime DepartureTimeActual, TransportType TransportType, int TripNumber, string OccupationLevel);