namespace Umbraco.AuthorizedServices.Models;

/// <summary>
/// Defines the result of an authorization operation with an external service.
/// </summary>
public class AuthorizationResult
{
    private AuthorizationResult()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeeded.
    /// </summary>
    public bool Success { get; private set; }

    /// <summary>
    /// Gets the error message in the event of a failed operation.
    /// </summary>
    public string ErrorMessage { get; private set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationResult"/> class for a successful operation.
    /// </summary>
    public static AuthorizationResult AsSuccess() => new() { Success = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationResult"/> class for a failed operation.
    /// </summary>
    public static AuthorizationResult AsError(string message) => new() { ErrorMessage = message };
}
