using dotVGN.Mapper.Base;
using dotVGN.Models;
using dotVGN.Models.API;
using dotVGN.Models.Enumerations;

namespace dotVGN.Mapper;

/// <summary>
/// Default implementation for converting departure API DTOs into domain models.
/// </summary>
public sealed class DepartureMapper : IDepartureMapper {
    /// <inheritdoc />
    public IReadOnlyList<Departure> Map(IEnumerable<ApiDepartureResponse.DepartureResponse> source) {
        var result = new List<Departure>();

        foreach (var r in source) {
            var transport = Enum.TryParse<TransportType>(r.Transport.Trim(), true, out var parsed) ? parsed : TransportType.Unknown;
            result.Add(new Departure(
                r.Line,
                r.StopPoint,
                r.Direction,
                r.DirectionDescription,
                r.Date.LocalDateTime,
                r.DepartureTimePlanned.LocalDateTime,
                r.DepartureTimeActual.LocalDateTime,
                transport,
                r.TripNumber,
                r.OccupationLevel));
        }

        return result;
    }
}