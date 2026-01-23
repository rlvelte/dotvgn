using DotVgn.Data.Contracts;
using DotVgn.Data.Enumerations;
using DotVgn.Data.Models;
using DotVgn.Mapper.Base;

namespace DotVgn.Mapper;

/// <summary>
/// Default implementation for converting departure API DTOs into domain models.
/// </summary>
internal sealed class DepartureMapper : IDepartureMapper {
    /// <inheritdoc />
    public IReadOnlyList<Departure> Map(IEnumerable<DepartureResponseContract.DepartureContract> source) {
        return source.Select(Map).ToList();
    }

    /// <inheritdoc />
    public Departure Map(DepartureResponseContract.DepartureContract source) {
        var transport = Enum.TryParse<TransportType>(source.Transport.Trim(), true, out var parsed) ? parsed : TransportType.Unknown;
        return new Departure(
            source.Line,
            source.StopPoint,
            source.Direction,
            source.DirectionDescription,
            source.Date.LocalDateTime,
            source.DepartureTimePlanned.LocalDateTime,
            source.DepartureTimeActual.LocalDateTime,
            transport,
            source.TripNumber,
            source.OccupationLevel);
    }
}