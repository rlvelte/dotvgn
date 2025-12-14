using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using DotVgn.Client.Additional;
using DotVgn.Models.Exceptions;
using DotVgn.Queries.Base;

namespace DotVgn.Client.Base;

/// <summary>
/// Base class for API clients.
/// </summary>
public class ClientBase {
    private readonly HttpClient _http;
    private readonly DotVgnClientOptions _options;

    /// <summary>
    /// Initializes a new instance of the ClientBase class.
    /// </summary>
    /// <param name="http">The http client to use.</param>
    /// <param name="options">The options to use.</param>
    protected ClientBase(HttpClient http, DotVgnClientOptions options) {
        _http = http;
        _http.BaseAddress = options.Value.BaseUri;
        
        _options = options;
    }

    /// <summary>
    /// Sends a batch of asynchronous requests to the specified paths and deserializes the response bodies to the specified type.
    /// </summary>
    /// <typeparam name="TItem">The item type to process.</typeparam>
    /// <typeparam name="TValue">The result type produced by the worker.</typeparam>
    /// <param name="queries">The items to process.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the request.</param>
    /// <returns>The task result contains the deserialized read-only list of (Key, Value) pairs.</returns>
    /// <exception cref="DotVgnApiException">
    /// Thrown if the HTTP response indicates a failure status code, or if the response body is empty or cannot be
    /// deserialized to the specified type.
    /// </exception>
    protected async Task<IReadOnlyList<(TItem Key, TValue Value)>> SendRequestsAsync<TItem, TValue>(IEnumerable<TItem> queries, CancellationToken cancellation) where TItem : IQuery {
        var result = new ConcurrentDictionary<TItem, TValue>();
        var semaphore = new SemaphoreSlim(_options.MaxParallelRequestsForBatch);

        var queryList = queries.ToList();
        var tasks = new List<Task>(queryList.Count);
        
        tasks.AddRange(queryList.Select(item => Task.Run(async () => {
            await semaphore.WaitAsync(cancellation).ConfigureAwait(false);
            
            try {
                result[item] = await SendRequestAsync<TValue>(item.GetRelativeUriExtension(), cancellation).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException) {
                throw;
            }
            finally {
                semaphore.Release();
            }
        }, cancellation)));

        await Task.WhenAll(tasks).ConfigureAwait(false);
        return result.Select(kv => (kv.Key, kv.Value)).ToList();
    }

    /// <summary>
    /// Sends an asynchronous request to the specified path and deserializes the response body to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the response body will be deserialized.</typeparam>
    /// <param name="path">The relative path to append to the base address for the HTTP request.</param>
    /// <param name="cancellation">A cancellation token that can be used to cancel the request.</param>
    /// <returns>The task result contains the deserialized response body of type <typeparamref name="T"/>.</returns>
    /// <exception cref="DotVgnApiException">
    /// Thrown if the HTTP response indicates a failure status code, or if the response body is empty or cannot be
    /// deserialized to the specified type.
    /// </exception>
    protected async Task<T> SendRequestAsync<T>(string path, CancellationToken cancellation) {
        ArgumentNullException.ThrowIfNull(_http.BaseAddress);

        var completeUri = new Uri(_http.BaseAddress, path);

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellation).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) {
            throw new DotVgnApiException(response.StatusCode, completeUri);
        }

        var result = JsonSerializer.Deserialize<T>(body, _options.Value.Json);
        return result ?? throw new DotVgnApiException(HttpStatusCode.OK, completeUri);
    }
}