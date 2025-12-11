using System.Net;

namespace dotVGN.Models.Exceptions;

/// <summary>
/// Represents a failed call to the upstream VGN API with diagnostic details.
/// </summary>
public sealed class DotVgnApiException(HttpStatusCode statusCode, Uri requestUri, string responseBody) : Exception($"VGN API request failed with {(int)statusCode} {statusCode} for {requestUri}");