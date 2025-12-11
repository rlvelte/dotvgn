using dotVGN.Models;
using dotVGN.Models.API;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using dotVGN.Client.Base;
using dotVGN.Mapper.Base;
using dotVGN.Models.Exceptions;
using Microsoft.Extensions.Options;
using dotVGN.Queries;

namespace dotVGN.Client;

/// <summary>
/// Default implementation of the VGN/VAG API client.
/// </summary>
public sealed class DotVgnClient : IDotVgnClient {
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;
    private readonly ILogger<DotVgnClient> _logger;
    private readonly IStationMapper _stationMapper;
    private readonly IDepartureMapper _departureMapper;

    /// <summary>
    /// Initializes a new instance of the DotVgnClient class.
    /// </summary>
    /// <param name="http">Configured HttpClient.</param>
    /// <param name="options">Configuration options for the client.</param>
    /// <param name="logger">Logger instance used for tracing and diagnostics.</param>
    /// <param name="stationMapper">Station mapping service.</param>
    /// <param name="departureMapper">Departure mapping service.</param>
    public DotVgnClient(HttpClient http, IOptions<DotVgnClientOptions> options, ILogger<DotVgnClient> logger, IStationMapper stationMapper, IDepartureMapper departureMapper) {
        _http = http;
        _logger = logger;
        _stationMapper = stationMapper;
        _departureMapper = departureMapper;

        _http.BaseAddress = options.Value.BaseUri;
        _json = options.Value.Json;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Station>> GetStationsAsync(StationQuery query, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(query);

        var response = await SendRequestAsync<ApiStationResponse>(query.GetRelativeUriExtension(), cancellation).ConfigureAwait(false);
        return _stationMapper.Map(response.Stations);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Departure>> GetDeparturesAsync(DepartureQuery query, CancellationToken cancellation = default) {
        ArgumentNullException.ThrowIfNull(query);

        var response = await SendRequestAsync<ApiDepartureResponse>(query.GetRelativeUriExtension(), cancellation).ConfigureAwait(false);
        return _departureMapper.Map(response.Departures).ToList();
    }

    /// <summary>
    /// Sends an asynchronous GET request to the specified path and deserializes the response body to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the response body will be deserialized.</typeparam>
    /// <param name="path">The relative path to append to the base address for the HTTP request.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the request.</param>
    /// <returns>The task result contains the deserialized response body of type <typeparamref name="T"/>.</returns>
    /// <exception cref="DotVgnApiException">
    /// Thrown if the HTTP response indicates a failure status code, or if the response body is empty or cannot be
    /// deserialized to the specified type.
    /// </exception>
    private async Task<T> SendRequestAsync<T>(string path, CancellationToken cancellation) {
        ArgumentNullException.ThrowIfNull(_http.BaseAddress);

        var completeUri = new Uri(_http.BaseAddress, path);

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellation).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) {
            throw new DotVgnApiException(response.StatusCode, completeUri, body);
        }

        var result = JsonSerializer.Deserialize<T>(body, _json);
        return result ?? throw new DotVgnApiException(HttpStatusCode.OK, completeUri, "Empty or invalid payload.");
    }
}