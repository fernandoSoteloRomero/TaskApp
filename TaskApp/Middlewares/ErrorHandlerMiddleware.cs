using System;
using System.Net;
using System.Text.Json;

namespace TaskApp.Middlewares;

public class ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
{
  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await next(context);
    }
    catch (Exception e)
    {
      logger.LogError(e, "An unexpected error occurred.");
    }
  }


  private static Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
  {
    // * por defecto 500 Internal Server Error
    var code = HttpStatusCode.InternalServerError;


    if (ex is UnauthorizedAccessException) code = HttpStatusCode.Unauthorized;
    else if (ex is ArgumentException) code = HttpStatusCode.BadRequest;

    var response = new
    {
      status = (int)code,
      error = ex.Message,
      traceId = httpContext.TraceIdentifier
    };


    var payload = JsonSerializer.Serialize(response);
    httpContext.Response.ContentType = "application/json";
    httpContext.Response.StatusCode = (int)code;
    return httpContext.Response.WriteAsync(payload);
  }
}
