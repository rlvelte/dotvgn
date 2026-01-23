using DotVgn.Data.Contracts;
using DotVgn.Data.Models;
using DotVgn.Mapper.Base;

namespace DotVgn.Mapper;

/// <summary>
/// Default implementation for converting trip API DTO into domain models.
/// </summary>
internal sealed class TripMapper : ITripMapper {
    /// <inheritdoc />
    public Trip Map(TripResponseContract source) {
        var stops = source.Stops.Select(stop => new TripStop(stop.Name, stop.VgnId, stop.Platform, stop.Latitude, stop.Longitude, stop.ArrivalTimeEstimated, stop.ArrivalTimeActual, stop.DepartureTimeEstimated, stop.DepartureTimeActual)).ToList();
        return new Trip(
            source.Line,
            source.Direction,
            source.DirectionDescription,
            stops
        );
    }
}
