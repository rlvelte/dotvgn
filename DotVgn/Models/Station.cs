using dotVGN.Models.Enumerations;

namespace dotVGN.Models;

/// <summary>
/// Represents a specific station in the VGN network.
/// </summary>
/// <param name="Name">The name of the station.</param>
/// <param name="StationId">The identifier of the VGN.</param>
/// <param name="Latitude">The latitude of the station on the map.</param>
/// <param name="Longitude">The longitude of the station on the map.</param>
/// <param name="Transports">All available means of transportation.</param>
public record Station(string Name, int StationId, double Latitude, double Longitude, IEnumerable<TransportType> Transports);