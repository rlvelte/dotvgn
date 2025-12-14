using DotVgn.Models;
using DotVgn.Models.API;

namespace DotVgn.Mapper.Base;

/// <summary>
/// Converts trip API DTO to a strongly typed Trip object with TripStops.
/// </summary>
internal interface ITripMapper {
    /// <summary>
    /// Maps an <see cref="ApiTripResponse"/> payload to a domain <see cref="Trip"/>.
    /// </summary>
    /// <param name="source">The API trip response to convert.</param>
    /// <returns>A mapped <see cref="Trip"/> instance. Never null.</returns>
    Trip Map(ApiTripResponse source);
}
