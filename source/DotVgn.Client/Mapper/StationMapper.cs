using DotVgn.Data.Contracts;
using DotVgn.Data.Enumerations;
using DotVgn.Data.Models;
using DotVgn.Mapper.Base;

namespace DotVgn.Mapper;

/// <summary>
/// Default implementation for converting station API DTOs into domain models.
/// </summary>
internal sealed class StationMapper : IStationMapper {
    /// <inheritdoc />
    public IReadOnlyList<Station> Map(IEnumerable<StationResponseContract.StationContract> source) {
        return source.Select(Map).ToList();
    }

    /// <inheritdoc />
    public Station Map(StationResponseContract.StationContract source) {
        var transports = string.IsNullOrWhiteSpace(source.Transports) ? [TransportType.Unknown] : source.Transports
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => Enum.TryParse<TransportType>(t.Trim(), true, out var parsed) ? parsed : TransportType.Unknown)
            .ToList();
        
        return new Station(
            source.Name,
            source.VgnId,
            source.Latitude,
            source.Longitude,
            transports);
    }
}