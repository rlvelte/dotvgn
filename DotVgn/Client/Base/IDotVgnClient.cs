using DotVgn.Models;
using DotVgn.Queries;
using DotVgn.Queries.Base;

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
    /// Asynchronously retrieves all stations that match the specified <see cref="StationQuery"/> lists criteria.
    /// </summary>
    /// <param name="queries">The criteria used to filter and select stations. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a tuple with the <see cref="StationQuery"/>> as a key and read-only list of <see cref="Station"/> as value. The list will be empty if no stations are found.</returns>
    Task<IReadOnlyList<(StationQuery Query, IReadOnlyList<Station> Stations)>> GetStationsAsync(IEnumerable<StationQuery> queries, CancellationToken cancellation = default);
    
    /// <summary>
    /// Asynchronously retrieves a list of departures that match the specified <see cref="DepartureQuery"/> criteria.
    /// </summary>
    /// <param name="query">The criteria used to filter and select departures. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a read-only list of <see cref="Departure"/> matching the query. The list will be empty if no departures are found.</returns>
    Task<IReadOnlyList<Departure>> GetDeparturesAsync(DepartureQuery query, CancellationToken cancellation = default);

    /// <summary>
    /// Asynchronously retrieves all departures that match the specified <see cref="DepartureQuery"/> lists criteria.
    /// </summary>
    /// <param name="queries">The criteria used to filter and select departures. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a tuple with the <see cref="DepartureQuery"/>> as a key and read-only list of <see cref="Departure"/> as value. The list will be empty if no departures are found.</returns>
    Task<IReadOnlyList<(DepartureQuery Query, IReadOnlyList<Departure> Departures)>> GetDeparturesAsync(IEnumerable<DepartureQuery> queries, CancellationToken cancellation = default);
    
    /// <summary>
    /// Asynchronously retrieves a trip that matches the specified <see cref="TripQuery"/> criteria.
    /// </summary>
    /// <param name="query">The criteria used to filter and select trips. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a <see cref="Trip"/> matching the query. Null if no trip was found.</returns>
    Task<Trip?> GetTripAsync(TripQuery query, CancellationToken cancellation = default);
    
    /// <summary>
    /// Asynchronously retrieves all trips that match the specified <see cref="TripQuery"/> lists criteria.
    /// </summary>
    /// <param name="queries">The criteria used to filter and select trips. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a tuple with the <see cref="TripQuery"/>> as a key and read-only list of <see cref="Trip"/> as value. The list will be empty if no trips are found.</returns>
    Task<IReadOnlyList<(TripQuery Query, IReadOnlyList<Trip> Trips)>> GetTripsAsync(IEnumerable<TripQuery> queries, CancellationToken cancellation = default);

}
