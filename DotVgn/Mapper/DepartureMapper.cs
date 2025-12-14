using DotVgn.Mapper.Base;
using DotVgn.Models;
using DotVgn.Models.API;
using DotVgn.Models.Enumerations;

namespace DotVgn.Mapper;

/// <summary>
/// Default implementation for converting departure API DTOs into domain models.
/// </summary>
internal sealed class DepartureMapper : IDepartureMapper {
    /// <inheritdoc />
    public IReadOnlyList<Departure> Map(IEnumerable<ApiDepartureResponse.DepartureResponse> source) {
        return source.Select(Map).ToList();
    }

    /// <inheritdoc />
    public Departure Map(ApiDepartureResponse.DepartureResponse source) {
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