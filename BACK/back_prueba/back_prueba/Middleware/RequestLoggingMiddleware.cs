using System.Diagnostics;

namespace back_prueba.Middleware
{
    public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();
                logger.LogInformation("{Method} {Path}{Query} -> {StatusCode} en {ElapsedMs} ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
