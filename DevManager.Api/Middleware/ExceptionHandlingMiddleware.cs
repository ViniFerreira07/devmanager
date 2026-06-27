using System.Net;
using System.Text.Json;
using DevManager.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DevManager.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Application.Common.Exceptions.ValidationException ex)
        {
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, ex.Code, ex.Message, ex.Errors.ToList());
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
            await WriteErrorResponse(context, HttpStatusCode.BadRequest, "VALIDATION_ERROR", "Validation failed.", errors);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteErrorResponse(context, HttpStatusCode.Unauthorized, "UNAUTHORIZED", ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteErrorResponse(context, HttpStatusCode.Forbidden, ex.Code, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorResponse(context, HttpStatusCode.NotFound, ex.Code, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteErrorResponse(context, HttpStatusCode.Conflict, ex.Code, ex.Message);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update exception");
            await WriteErrorResponse(context, HttpStatusCode.Conflict, "DATABASE_ERROR", "A database conflict occurred. The operation could not be completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponse(context, HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, HttpStatusCode statusCode, string code, string message, List<string>? errors = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            success = false,
            code,
            message,
            errors = errors ?? new List<string>()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}