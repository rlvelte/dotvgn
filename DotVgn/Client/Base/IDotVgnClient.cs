using DotVgn.Models;
using DotVgn.Queries;

namespace DotVgn.Client.Base;

/// <summary>
/// Abstraction for the VGN API client.
/// </summary>
public interface IDotVgnClient {
    /// <summary>
    /// Asynchronously retrieves a list of stations that match the specified <see cref="StationQuery"/> criteria.
    /// </summary>
    /// <param name="query">The criteria used to filter and search for stations. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a read-only list of <see cref="Station"/> matching the query. The list will be empty if no stations are found.</returns>
    Task<IReadOnlyList<Station>> GetStationsAsync(StationQuery query, CancellationToken cancellation = default);

    /// <summary>
    /// Asynchronously retrieves a list of departures that match the specified <see cref="DepartureQuery"/> criteria.
    /// </summary>
    /// <param name="query">The criteria used to filter and select departures. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a read-only list of <see cref="Departure"/> matching the query. The list will be empty if no departures are found.</returns>
    Task<IReadOnlyList<Departure>> GetDeparturesAsync(DepartureQuery query, CancellationToken cancellation = default);
}
