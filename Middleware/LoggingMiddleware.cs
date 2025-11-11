using System.Diagnostics;

namespace mvcLab.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _next(context);
                stopwatch.Stop();

                var logMessage = $"Request {context.Request.Method} {context.Request.Path} completed in {stopwatch.ElapsedMilliseconds}ms with status code {context.Response.StatusCode}";
                _logger.LogInformation(logMessage);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"Request {context.Request.Method} {context.Request.Path} failed after {stopwatch.ElapsedMilliseconds}ms");
                throw;
            }
        }
    }

    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}