using System.Net;
using CarRent.Application.Exceptions;
using FluentValidation;
using ValidationException = FluentValidation.ValidationException;

namespace CarRent.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception: {Message}",
                ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        object response;

        if (exception is ValidationException fluentException)
        {
            statusCode = (int)HttpStatusCode.UnprocessableEntity;

            response = new
            {
                error = "Validation failed.",
                errors = fluentException.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                })
            };
        }
        else if (exception is CarRent.Application.Exceptions.ValidationException domainValidationException)
        {
            statusCode = (int)HttpStatusCode.UnprocessableEntity;

            response = new
            {
                error = domainValidationException.Message
            };
        }
        else if (exception is ConflictException conflictException)
        {
            statusCode = (int)HttpStatusCode.Conflict;

            response = new
            {
                error = conflictException.Message
            };
        }
        else if (exception is UnauthorizedException unauthorizedException)
        {
            statusCode = (int)HttpStatusCode.Unauthorized;

            response = new
            {
                error = unauthorizedException.Message
            };
        }
        else if (exception is NotFoundException notFoundException)
        {
            statusCode = (int)HttpStatusCode.NotFound;

            response = new
            {
                error = notFoundException.Message
            };
        }
        else if (exception is NotImplementedException)
        {
            statusCode = (int)HttpStatusCode.NotImplemented;

            response = new
            {
                error = "This feature is not yet implemented."
            };
        }
        else
        {
            statusCode = (int)HttpStatusCode.InternalServerError;

            response = new
            {
                error = "An unexpected error occurred."
            };
        }

        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(response);
    }
}