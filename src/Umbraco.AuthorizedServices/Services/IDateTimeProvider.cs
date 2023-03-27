namespace Umbraco.AuthorizedServices.Services;

/// <summary>
/// Provides operations on current date and time.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC datetime.
    /// </summary>
    DateTime UtcNow();
}
