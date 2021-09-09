using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace unbooru.Web
{
    public class ErrorHandlingMiddleware
    {
        readonly RequestDelegate _next;
        static ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            _logger.LogError(exception, "{Error}", exception);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "text/plain";

            await context.Response.WriteAsync(exception.ToString());
        }
    }
}