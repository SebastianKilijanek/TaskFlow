using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TaskFlow.Application.Common.Exceptions;

namespace TaskFlow.API.Middleware;

public class ExceptionToProblemDetailsHandler(ILogger<ExceptionToProblemDetailsHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            logger.LogDebug("Request cancelled.");
            return true;
        }

        var (status, title) = Map(exception);

        if (status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception for {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
        else
            logger.LogWarning("{Title} for {Method} {Path}: {Message}",
                title, httpContext.Request.Method, httpContext.Request.Path, exception.Message);

        if (httpContext.Response.HasStarted)
            return true;

        httpContext.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status >= StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier
            }
        };

        var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });

        return true;
    }

    private static (int Status, string Title) Map(Exception exception)
    {
        return exception switch
        {
            ValidationException or BadRequestException
                => (StatusCodes.Status400BadRequest, "Bad request"),
            UnauthorizedAccessException
                => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ForbiddenAccessException
                => (StatusCodes.Status403Forbidden, "Forbidden"),
            NotFoundException
                => (StatusCodes.Status404NotFound, "Not found"),
            ConflictException
                => (StatusCodes.Status409Conflict, "Conflict"),
            DbUpdateConcurrencyException
                => (StatusCodes.Status409Conflict, "Conflict"),
            DbUpdateException when exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation
            }
                => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal server error")
        };
    }
}