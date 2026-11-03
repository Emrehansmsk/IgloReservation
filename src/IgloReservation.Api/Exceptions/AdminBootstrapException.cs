namespace IgloReservation.Api.Exceptions;

/// <summary>
/// Raised for AdminBootstrap configuration or identity failures that should fail application startup.
/// Messages must never include passwords or other secrets.
/// </summary>
public sealed class AdminBootstrapException : InvalidOperationException
{
    public AdminBootstrapException(string message)
        : base(message)
    {
    }

    public AdminBootstrapException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
