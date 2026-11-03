using System.Text.Json;
using IgloReservation.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IgloReservation.Api.Middleware;

/// <summary>
/// Maps unhandled exceptions to ProblemDetails. Production responses omit stack traces and DB details.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (status, title, detail) = Map(exception);

        if (status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Request failed with {StatusCode} for {Method} {Path}", status, context.Request.Method, context.Request.Path);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        if (_environment.IsDevelopment() && status >= StatusCodes.Status500InternalServerError)
        {
            problem.Extensions["exception"] = exception.GetType().Name;
            problem.Extensions["stackTrace"] = exception.ToString();
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }

    private (int Status, string Title, string Detail) Map(Exception exception) =>
        exception switch
        {
            ReservationConflictException ex => (
                StatusCodes.Status409Conflict,
                "Reservation conflict",
                ex.Message),
            DbUpdateException ex when IsPostgresExclusionViolation(ex) => (
                StatusCodes.Status409Conflict,
                "Reservation conflict",
                "An active reservation (Pending or Confirmed) overlaps this time range for the unit."),
            BusinessRuleException ex => (
                StatusCodes.Status400BadRequest,
                "Business rule violation",
                ex.Message),
            UnauthorizedAccessException ex => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                string.IsNullOrWhiteSpace(ex.Message)
                    ? "Kimlik doğrulama gerekli veya oturum geçersiz."
                    : ex.Message),
            ArgumentException ex => (
                StatusCodes.Status400BadRequest,
                "Invalid request",
                ex.Message),
            InvalidOperationException ex when IsSafeClientMessage(ex) => (
                StatusCodes.Status400BadRequest,
                "Invalid operation",
                ex.Message),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.")
        };

    private static bool IsPostgresExclusionViolation(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres && postgres.SqlState == PostgresErrorCodes.ExclusionViolation)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSafeClientMessage(InvalidOperationException ex) =>
        !ex.Message.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)
        && !ex.Message.Contains("SQL", StringComparison.OrdinalIgnoreCase)
        && !ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase);
}
