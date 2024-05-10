using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using API.Errors;

namespace API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    // This is an asynchronous method that handles HTTP requests.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);// Call the next middleware in the pipeline.
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message); // Log the error using the logger.

            context.Response.ContentType = "application/json";// Set the response content type to JSON.
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;// Set the response status code to 500 (Internal Server Error).

            // Create an ApiException response object.
            // If in development mode, include the exception stack trace; otherwise, provide a generic error message.
            ApiException response = _env.IsDevelopment() 
                ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString()) 
                : new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");

            // Configure JsonSerializer options (e.g., camelCase property naming).
            JsonSerializerOptions options = new JsonSerializerOptions{
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Serialize the ApiException response object to JSON.
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}
