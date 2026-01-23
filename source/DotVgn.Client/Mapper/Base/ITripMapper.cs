using DotVgn.Data.Contracts;
using DotVgn.Data.Models;

namespace DotVgn.Mapper.Base;

/// <summary>
/// Converts trip API DTO to a strongly typed Trip object with TripStops.
/// </summary>
public interface ITripMapper {
    /// <summary>
    /// Maps an <see cref="TripResponseContract"/> payload to a domain <see cref="Trip"/>.
    /// </summary>
    /// <param name="source">The API trip response to convert.</param>
    /// <returns>A mapped <see cref="Trip"/> instance. Never null.</returns>
    Trip Map(TripResponseContract source);
}
