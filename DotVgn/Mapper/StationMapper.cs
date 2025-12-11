using DotVgn.Mapper.Base;
using DotVgn.Models;
using DotVgn.Models.API;
using DotVgn.Models.Enumerations;

namespace DotVgn.Mapper;

/// <summary>
/// Default implementation for converting station API DTOs into domain models.
/// </summary>
public sealed class StationMapper : IStationMapper {
    /// <inheritdoc />
    public IReadOnlyList<Station> Map(IEnumerable<ApiStationResponse.StationResponse> source) {
        var result = new List<Station>();

        foreach (var response in source) {
            var transports = string.IsNullOrWhiteSpace(response.Transports) ? [TransportType.Unknown] : response.Transports
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => Enum.TryParse<TransportType>(t.Trim(), true, out var parsed) ? parsed : TransportType.Unknown)
                .ToList();

            result.Add(new Station(
                response.Name,
                response.VgnId,
                response.Latitude,
                response.Longitude,
                transports));
        }

        return result;
    }
}