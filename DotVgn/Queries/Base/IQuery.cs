namespace dotVGN.Queries.Base;

/// <summary>
/// Defines a contract for building a relative URI extension string that includes query parameters.
/// </summary>
public interface IQuery {
    /// <summary>
    /// Gets a uri extension string with included query parameters.
    /// </summary>
    /// <returns>A string representing the relative uri extension, including parameters.</returns>
    public string GetRelativeUriExtension();
}