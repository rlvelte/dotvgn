using DotVgn.Models;
using DotVgn.Models.API;

namespace DotVgn.Mapper.Base;

/// <summary>
/// Converts station API DTOs into strongly typed station objects.
/// </summary>
public interface IStationMapper {
    /// <summary>
    /// Maps a collection of <see cref="ApiStationResponse"/> to a read-only list of domain <see cref="Station"/> instances.
    /// </summary>
    /// <param name="source">The collection of API stations response objects to convert. Cannot be null.</param>
    /// <returns>A read-only list of <see cref="Station"/> objects representing the mapped stations. The list will be empty if the source collection contains no items.</returns>
    IReadOnlyList<Station> Map(IEnumerable<ApiStationResponse.StationResponse> source);
}