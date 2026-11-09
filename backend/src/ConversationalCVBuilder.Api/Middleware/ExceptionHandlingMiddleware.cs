using System.Text.Json;
using ConversationalCVBuilder.Application.Common.Exceptions;
using ConversationalCVBuilder.Domain.Common;
using FluentValidation;

namespace ConversationalCVBuilder.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var (statusCode, title, errors) = exception switch
            {
                ValidationException validationException => (
                    StatusCodes.Status400BadRequest,
                    "Validation failed",
                    validationException.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(error => error.ErrorMessage).Distinct().ToArray())),
                NotFoundException => (
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    (IReadOnlyDictionary<string, string[]>?)null),
                DomainException or ArgumentException or InvalidDataException => (
                    StatusCodes.Status400BadRequest,
                    "Invalid request",
                    (IReadOnlyDictionary<string, string[]>?)null),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred",
                    (IReadOnlyDictionary<string, string[]>?)null)
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            }
            else
            {
                logger.LogWarning(exception, "Request failed with {StatusCode} for {Method} {Path}",
                    statusCode, context.Request.Method, context.Request.Path);
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                new { title, status = statusCode, errors },
                cancellationToken: context.RequestAborted);
        }
    }
}
