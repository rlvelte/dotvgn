using DotVgn.Models;
using DotVgn.Models.API;

namespace DotVgn.Mapper.Base;

/// <summary>
/// Converts departure API DTOs into strongly typed departure objects.
/// </summary>
public interface IDepartureMapper {
    /// <summary>
    /// Maps a collection of <see cref="ApiDepartureResponse"/> to a read-only list of domain <see cref="Departure"/> instances.
    /// </summary>
    /// <param name="source">The collection of API departure response objects to convert. Cannot be null.</param>
    /// <returns>A read-only list of <see cref="Departure"/> objects representing the mapped departures. The list will be empty if the source collection contains no items.</returns>
    IReadOnlyList<Departure> Map(IEnumerable<ApiDepartureResponse.DepartureResponse> source);
}