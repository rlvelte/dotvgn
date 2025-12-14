using DotVgn.Models;
using DotVgn.Models.API;
using Microsoft.Extensions.Logging;
using DotVgn.Client.Base;
using DotVgn.Mapper;
using DotVgn.Mapper.Base;
using Microsoft.Extensions.Options;
using DotVgn.Queries;
using System.Collections.Concurrent;
using DotVgn.Client.Additional;

namespace DotVgn.Client;

/// <summary>
/// Default implementation of the VGN/VAG API client.
/// </summary>
public sealed class DotVgnClient : ClientBase, IDotVgnClient {
    private readonly ILogger<DotVgnClient> _logger;
    
    private readonly ITripMapper _tripMapper;
    private readonly IStationMapper _stationMapper;
    private readonly IDepartureMapper _departureMapper;
    
    private readonly TimeSpan _stationCacheTtl;
    private readonly ConcurrentDictionary<int, (DateTimeOffset ExpiresAt, Station Data)> _stationCache = new();

    /// <summary>
    /// Initializes a new instance of the DotVgnClient class.
    /// </summary>
    /// <param name="http">Configured HttpClient.</param>
    /// <param name="options">Configuration options for the client.</param>
    /// <param name="logger">Logger instance used for tracing and diagnostics.</param>
    public DotVgnClient(HttpClient http, IOptions<DotVgnClientOptions> options, ILogger<DotVgnClient> logger) : base(http, options.Value){
        _stationCacheTtl = options.Value.StationCacheTtl;
        _logger = logger;
        
        _stationMapper = new StationMapper();
        _departureMapper = new DepartureMapper();
        _tripMapper = new TripMapper();
    }

    /// <summary>
    /// Initializes a new instance of the DotVgnClient class.
    /// </summary>
    /// <param name="http">Configured HttpClient.</param>
    /// <param name="options">Configuration options for the client.</param>
    /// <param name="logger">Logger instance used for tracing and diagnostics.</param>
    public DotVgnClient(HttpClient http, DotVgnClientOptions options, ILogger<DotVgnClient> logger) : this(http, Options.Create(options), logger) { }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Station>> GetStationsAsync(StationQuery query, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(query);
        var path = query.GetRelativeUriExtension();

        var response = await SendRequestAsync<ApiStationResponse>(path, cancellation).ConfigureAwait(false);
        var mapped = _stationMapper.Map(response.Stations);

        if (_stationCacheTtl <= TimeSpan.Zero) {
            return mapped;
        }

        var now = DateTimeOffset.UtcNow;
        var expires = now.Add(_stationCacheTtl);
        var stations = new List<Station>(mapped.Count);

        foreach (var station in mapped) {
            if (_stationCache.TryGetValue(station.StationId, out var entry) && entry.ExpiresAt > now) {
                stations.Add(entry.Data);
            } else {
                _stationCache[station.StationId] = (expires, station);
                stations.Add(station);
            }
        }

        _logger.LogDebug("Stations merged with per-id cache for {Path}; {Count} entries", path, stations.Count);
        return stations;

    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<(StationQuery Query, IReadOnlyList<Station> Stations)>> GetStationsAsync(IEnumerable<StationQuery> queries, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(queries);
        var list = queries.ToList();
        if (list.Count == 0) {
            throw new ArgumentException("Queries must not be null or empty.", nameof(queries));
        }

        var responses = await SendRequestsAsync<StationQuery, ApiStationResponse>(list, cancellation).ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;
        var expires = now.Add(_stationCacheTtl);

        var result = new List<(StationQuery, IReadOnlyList<Station>)>(responses.Count);
        foreach (var kv in responses) {
            var mapped = _stationMapper.Map(kv.Value.Stations);
            var merged = MergeWithCache(mapped);
            
            result.Add((kv.Key, merged));
        }
        return result;

        IReadOnlyList<Station> MergeWithCache(IEnumerable<Station> mapped) {
            if (_stationCacheTtl <= TimeSpan.Zero) return mapped.ToList();

            var stations = new List<Station>();
            foreach (var station in mapped) {
                if (_stationCache.TryGetValue(station.StationId, out var entry) && entry.ExpiresAt > now) {
                    stations.Add(entry.Data);
                } else {
                    _stationCache[station.StationId] = (expires, station);
                    stations.Add(station);
                }
            }
            return stations;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Departure>> GetDeparturesAsync(DepartureQuery query, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(query);

        var response = await SendRequestAsync<ApiDepartureResponse>(query.GetRelativeUriExtension(), cancellation).ConfigureAwait(false);
        return _departureMapper.Map(response.Departures);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<(DepartureQuery Query, IReadOnlyList<Departure> Departures)>> GetDeparturesAsync(IEnumerable<DepartureQuery> queries, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(queries);
        
        var departureQueries = queries.ToList();
        if (departureQueries.Count == 0) {
            throw new ArgumentException("Queries must not be null or empty.", nameof(queries));
        }
        
        var responses = await SendRequestsAsync<DepartureQuery, ApiDepartureResponse>(departureQueries, cancellation).ConfigureAwait(false);
        return responses.Select(kv => (kv.Key, _departureMapper.Map(kv.Value.Departures))).ToList();
    }

    /// <inheritdoc />
    public async Task<Trip?> GetTripAsync(TripQuery query, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(query);

        var response = await SendRequestAsync<ApiTripResponse>(query.GetRelativeUriExtension(), cancellation).ConfigureAwait(false);
        return _tripMapper.Map(response);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<(TripQuery Query, IReadOnlyList<Trip> Trips)>> GetTripsAsync(IEnumerable<TripQuery> queries, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(queries);

        var tripQueries = queries.ToList();
        if (tripQueries.Count == 0) {
            throw new ArgumentException("Queries must not be null or empty.", nameof(queries));
        }

        var responses = await SendRequestsAsync<TripQuery, ApiTripResponse>(tripQueries, cancellation).ConfigureAwait(false);
        return responses
            .Select(kv => (kv.Key, (IReadOnlyList<Trip>) new List<Trip> { _tripMapper.Map(kv.Value) }))
            .ToList();
    }
}