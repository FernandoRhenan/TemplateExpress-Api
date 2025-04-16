using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TemplateExpress.Api.Middlewares;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{

    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger) => _logger = logger;   
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);

            ProblemDetails details = new()
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Internal Server Error",
                Type = "Internal Server Error",
                Detail = "An internal server error has occurred."
            };

            string jsonDetails = JsonSerializer.Serialize(details);
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            await context.Response.WriteAsync(jsonDetails);
        }
    }

}