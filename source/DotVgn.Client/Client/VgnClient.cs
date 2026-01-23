using DotVgn.Client.Base;
using DotVgn.Mapper;
using DotVgn.Mapper.Base;
using DotVgn.Queries;
using DotVgn.Data.Contracts;
using DotVgn.Data.Models;

namespace DotVgn.Client;

/// <summary>
/// Default implementation of the VGN/VAG API client.
/// </summary>
public sealed class VgnClient : ClientBase {
    private readonly ITripMapper _tripMapper;
    private readonly IStationMapper _stationMapper;
    private readonly IDepartureMapper _departureMapper;

    /// <summary>
    /// Client for accessing VAG/VGN-API.
    /// </summary>
    /// <param name="http">The HTTP client used for requests.</param>
    /// <param name="departureMapper">The mapper for departures.</param>
    /// <param name="tripMapper">The mapper for trips.</param>
    /// <param name="stationMapper">The mapper for stations.</param>
    public VgnClient(HttpClient http, IDepartureMapper departureMapper, ITripMapper tripMapper, IStationMapper stationMapper) 
        : base(http){
        _stationMapper = stationMapper;
        _departureMapper = departureMapper;
        _tripMapper = tripMapper;
    }
    
    /// <summary>
    /// Client for accessing VAG/VGN-API.
    /// </summary>
    /// <param name="options">Configuration options for the client.</param>
    public VgnClient(ClientOptions? options = null) : base(options){
        _stationMapper = new StationMapper();
        _departureMapper = new DepartureMapper();
        _tripMapper = new TripMapper();
    }
    
    /// <summary>
    /// Asynchronously retrieves a list of stations that match the specified <see cref="StationQuery"/> criteria.
    /// </summary>
    /// <param name="query">The criteria used to filter and search for stations. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a read-only list of <see cref="Station"/> matching the query. The list will be empty if no stations are found.</returns>
    public async Task<IReadOnlyList<Station>> GetStationsAsync(StationQuery query, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(query);
        var path = query.GetRelativeUriExtension();

        var response = await SendRequestAsync<StationResponseContract>(path, cancellation);
        var mapped = _stationMapper.Map(response.Stations);
        
        var stations = new List<Station>(mapped.Count);
        foreach (var station in mapped) {
            stations.Add(station);
        }

        return stations;
    }

    /// <summary>
    /// Asynchronously retrieves all stations that match the specified <see cref="StationQuery"/> lists criteria.
    /// </summary>
    /// <param name="queries">The criteria used to filter and select stations. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a tuple with the <see cref="StationQuery"/>> as a key and read-only list of <see cref="Station"/> as value. The list will be empty if no stations are found.</returns>
    public async Task<IReadOnlyList<(StationQuery Query, IReadOnlyList<Station> Stations)>> GetStationsAsync(IEnumerable<StationQuery> queries, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(queries);
        var list = queries.ToList();
        if (list.Count == 0) {
            throw new ArgumentException("Queries must not be null or empty.", nameof(queries));
        }

        var responses = await SendRequestsAsync<StationQuery, StationResponseContract>(list, cancellation);
        
        var result = new List<(StationQuery, IReadOnlyList<Station>)>(responses.Count);
        foreach (var kv in responses) {
            var mapped = _stationMapper.Map(kv.Value.Stations);
            result.Add((kv.Key, mapped));
        }
        return result;
    }

    /// <summary>
    /// Asynchronously retrieves a list of departures that match the specified <see cref="DepartureQuery"/> criteria.
    /// </summary>
    /// <param name="query">The criteria used to filter and select departures. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a read-only list of <see cref="Departure"/> matching the query. The list will be empty if no departures are found.</returns>
    public async Task<IReadOnlyList<Departure>> GetDeparturesAsync(DepartureQuery query, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(query);

        var response = await SendRequestAsync<DepartureResponseContract>(query.GetRelativeUriExtension(), cancellation);
        return _departureMapper.Map(response.Departures);
    }

    /// <summary>
    /// Asynchronously retrieves all departures that match the specified <see cref="DepartureQuery"/> lists criteria.
    /// </summary>
    /// <param name="queries">The criteria used to filter and select departures. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a tuple with the <see cref="DepartureQuery"/>> as a key and read-only list of <see cref="Departure"/> as value. The list will be empty if no departures are found.</returns>
    public async Task<IReadOnlyList<(DepartureQuery Query, IReadOnlyList<Departure> Departures)>> GetDeparturesAsync(IEnumerable<DepartureQuery> queries, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(queries);
        
        var departureQueries = queries.ToList();
        if (departureQueries.Count == 0) {
            throw new ArgumentException("Queries must not be null or empty.", nameof(queries));
        }
        
        var responses = await SendRequestsAsync<DepartureQuery, DepartureResponseContract>(departureQueries, cancellation);
        return responses.Select(kv => (kv.Key, _departureMapper.Map(kv.Value.Departures))).ToList();
    }

    /// <summary>
    /// Asynchronously retrieves a trip that matches the specified <see cref="TripQuery"/> criteria.
    /// </summary>
    /// <param name="query">The criteria used to filter and select trips. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a <see cref="Trip"/> matching the query. Null if no trip was found.</returns>
    public async Task<Trip?> GetTripAsync(TripQuery query, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(query);

        var response = await SendRequestAsync<TripResponseContract>(query.GetRelativeUriExtension(), cancellation);
        return _tripMapper.Map(response);
    }

    /// <summary>
    /// Asynchronously retrieves all trips that match the specified <see cref="TripQuery"/> lists criteria.
    /// </summary>
    /// <param name="queries">The criteria used to filter and select trips. Cannot be null.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>The task result contains a tuple with the <see cref="TripQuery"/>> as a key and read-only list of <see cref="Trip"/> as value. The list will be empty if no trips are found.</returns>
    public async Task<IReadOnlyList<(TripQuery Query, IReadOnlyList<Trip> Trips)>> GetTripsAsync(IEnumerable<TripQuery> queries, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(queries);

        var tripQueries = queries.ToList();
        if (tripQueries.Count == 0) {
            throw new ArgumentException("Queries must not be null or empty.", nameof(queries));
        }

        var responses = await SendRequestsAsync<TripQuery, TripResponseContract>(tripQueries, cancellation);
        return responses
            .Select(kv => (kv.Key, (IReadOnlyList<Trip>) new List<Trip> {
                _tripMapper.Map(kv.Value)
            }))
            .ToList();
    }
}