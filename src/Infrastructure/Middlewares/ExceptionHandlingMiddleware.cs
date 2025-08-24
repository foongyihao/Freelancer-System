using CDN.Freelancers.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

namespace CDN.Freelancers.Infrastructure.Middlewares;

/// <summary>
/// ASP.NET Core middleware that converts thrown exceptions into RFC 7807 ProblemDetails responses.
/// Centralizes error handling and keeps controllers/repositories clean of HTTP concerns.
/// </summary>
public class ExceptionHandlingMiddleware {
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next) { _next = next; }

    /// <summary>
    /// Invokes the middleware and handles any exceptions thrown by downstream components.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context) {
        try {
            await _next(context);
        }
        catch (Exception ex) {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Maps known exceptions to ProblemDetails and writes the response.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="ex">The caught exception.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    private static Task HandleExceptionAsync(HttpContext context, Exception ex) {
        int statusCode;
        var problem = new ProblemDetails {
            Title = "An error occurred",
            Detail = ex.Message,
            Instance = context.Request.Path
        };

        switch (ex) {
            case EntityNotFoundException notFoundEx:
                statusCode = StatusCodes.Status404NotFound;
                problem.Title = "Entity not found";
                problem.Extensions["entity"] = notFoundEx.EntityName;
                problem.Extensions["key"] = notFoundEx.Key;
                break;

            case DuplicateRecordException duplicateEx:
                statusCode = StatusCodes.Status409Conflict;
                problem.Title = "Duplicate record";
                problem.Extensions["entity"] = duplicateEx.EntityName;
                problem.Extensions["key"] = duplicateEx.Key;
                break;


            case UnauthorizedAccessException:
                statusCode = StatusCodes.Status401Unauthorized;
                problem.Title = "Unauthorized";
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                problem.Title = "Unexpected error";
                break;
        }

        problem.Status = statusCode;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(problem));
    }
}

/// <summary>
/// Extension methods for registering the <see cref="ExceptionHandlingMiddleware"/>.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions {
    /// <summary>
    /// Registers the global exception handling middleware.
    /// </summary>
    /// <param name="app">The application pipeline builder.</param>
    /// <returns>The same application builder instance.</returns>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app) => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
