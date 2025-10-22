using Common.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace SPA_Comments.Server.Middlewares;

/// <summary>
/// Клас middleware конвеєру HTTP контексту для перехоплення виключних ситуацій
/// </summary>
/// <param name="next">Делегат конвеєра HTTP request</param>
/// <param name="logger">Об'єкт для логування</param>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    /// <summary>
    /// Метод обробки Middleware конвеєру HTTP контексту для перехоплення виключних ситуацій
    /// </summary>
    /// <description>
    /// Єдине місце перехоплення, обробки, логування виключних ситуацій.
    /// </description>
    /// <param name="httpContext">Об'єкт HTTP контексту перехопленого виключенням</param>
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var errorResponse = new ErrorResponse() { Success = false };

        switch (exception)
        {
            case BaseException ex:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                if (ex.HttpCode is not null)
                {
                    context.Response.StatusCode = (int)ex.HttpCode;
                }
                errorResponse = BaseExceptionsHandler(ex);
                break;
            case ValidationException ex:
                context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                errorResponse = BaseExceptionsHandler(ex, "Validation error");
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Title = "Unhandled exception type !";
                errorResponse.Message = exception.Message;
                break;
        }

        _logger.LogError(exception, exception.Message);

        var result = JsonSerializer.Serialize(errorResponse);

        await context.Response.WriteAsync(result);
    }


    private static ErrorResponse BaseExceptionsHandler(Exception exception, string customTitle)
    {
        var ex = exception as BaseException;

        var errorResponse = new ErrorResponse()
        {
            Title = customTitle,
            Success = false,
            Message = exception.Message
        };

        if (ex is not null && ex!.Place is not null)
        {
            errorResponse.Place = ex.Place;
        }

        return errorResponse;
    }

    private static ErrorResponse BaseExceptionsHandler(Exception exception)
    {
        var ex = exception as BaseException;

        var errorResponse = new ErrorResponse()
        {
            Title = ex?.Title,
            Success = false,
            Message = ex?.Message,
            Place = ex?.Place
        };

        return errorResponse;
    }
}