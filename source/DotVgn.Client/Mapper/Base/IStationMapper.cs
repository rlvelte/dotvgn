using DotVgn.Data.Contracts;
using DotVgn.Data.Models;

namespace DotVgn.Mapper.Base;

/// <summary>
/// Converts station API DTOs into strongly typed station objects.
/// </summary>
public interface IStationMapper {
    /// <summary>
    /// Maps a collection of <see cref="StationResponseContract"/> to a read-only list of domain <see cref="Station"/> instances.
    /// </summary>
    /// <param name="source">The collection of API stations response objects to convert. Cannot be null.</param>
    /// <returns>A read-only list of <see cref="Station"/> objects representing the mapped stations. The list will be empty if the source collection contains no items.</returns>
    IReadOnlyList<Station> Map(IEnumerable<StationResponseContract.StationContract> source);

    /// <summary>
    /// Maps a single API station response object to a domain <see cref="Station"/> instance.
    /// </summary>
    /// <param name="source">The API station response object to convert.</param>
    /// <returns>The mapped <see cref="Station"/>.</returns>
    Station Map(StationResponseContract.StationContract source);
}