using System.ComponentModel.DataAnnotations;
using TaskFlow.Application.Common.Exceptions;

namespace TaskFlow.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception has occurred: {Message}", ex.Message);

            context.Response.ContentType = "application/json";

            var statusCode = GetStatusCodeFromException(ex);
            context.Response.StatusCode = statusCode;

            var response = new
            {
                error = ex.GetType().Name.Replace("Exception", string.Empty),
                message = ex.Message,
                status = statusCode
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }

    private static int GetStatusCodeFromException(Exception exception)
    {
        return exception switch
        {
            ValidationException or BadRequestException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ForbiddenAccessException => StatusCodes.Status403Forbidden,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}