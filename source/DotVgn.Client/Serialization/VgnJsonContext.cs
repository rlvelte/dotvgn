using System.Text.Json.Serialization;
using DotVgn.Common.Contracts;

namespace DotVgn.Client.Serialization;

/// <summary>
/// Source-generated JSON serializer context for the VAG/VGN API contracts.
/// Enables reflection-free JSON serialization, required for native AOT.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(StationResponseContract))]
[JsonSerializable(typeof(DepartureResponseContract))]
[JsonSerializable(typeof(TripResponseContract))]
internal sealed partial class VgnJsonContext : JsonSerializerContext;
