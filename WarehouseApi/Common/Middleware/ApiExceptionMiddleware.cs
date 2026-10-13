using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WarehouseApi.Common.Exceptions;

namespace WarehouseApi.Common.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
            {
                context.Response.StatusCode = 499;
                return;
            }

            logger.LogWarning(exception, "API request failed: {Message}", exception.Message);
            context.Response.ContentType = "application/problem+json";

            switch (exception)
            {
                case ValidationException validationException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    var errors = validationException.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
                    await context.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Request validation failed",
                        Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1"
                    });
                    break;

                case NotFoundException notFoundException:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Resource not found",
                        Detail = notFoundException.Message
                    });
                    break;

                case InvalidOperationException invalidOperationException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Business rule violation",
                        Detail = invalidOperationException.Message
                    });
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "An unexpected error occurred",
                        Detail = "The request could not be completed."
                    });
                    break;
            }
        }
    }
}
