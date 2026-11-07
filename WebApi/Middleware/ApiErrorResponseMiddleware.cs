using Microsoft.AspNetCore.Http;
using WebApi.Config;
using WebApi.ViewModels;

namespace WebApi.Middleware
{
    /// <summary>
    /// Makes every error response under /api use ErrorViewModel: unhandled exceptions become 500 and bodyless framework errors receive a body.
    /// </summary>
    public sealed class ApiErrorResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiErrorResponseMiddleware> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="ApiErrorResponseMiddleware"/>.
        /// </summary>
        /// <param name="next">Next middleware in the pipeline.</param>
        /// <param name="logger">Logger used to record unhandled exceptions.</param>
        public ApiErrorResponseMiddleware(RequestDelegate next, ILogger<ApiErrorResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes the request and normalizes the error response when needed.
        /// </summary>
        /// <param name="context">HTTP context of the current request.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var originalBody = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                await _next(context);
            }
            catch (Exception ex) when (!context.RequestAborted.IsCancellationRequested)
            {
                var safeMethod = SanitizeForLog(context.Request.Method);
                var safePath = SanitizeForLog(context.Request.Path.Value);
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}.", safeMethod, safePath);
                context.Response.Body = originalBody;
                await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, ApiErrorMessages.UnexpectedError);
                return;
            }

            context.Response.Body = originalBody;

            if (context.Response.StatusCode >= StatusCodes.Status400BadRequest && IsFrameworkErrorBody(context.Response))
            {
                await WriteErrorAsync(context, context.Response.StatusCode, MessageFor(context.Response.StatusCode));
                return;
            }

            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody, context.RequestAborted);
        }

        private static string SanitizeForLog(string? value) =>
            string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("\r", " ").Replace("\n", " ");

        private static bool IsFrameworkErrorBody(HttpResponse response) =>
            response.ContentType == null ||
            response.ContentType.StartsWith("application/problem+json", StringComparison.OrdinalIgnoreCase);

        private static string MessageFor(int statusCode) => statusCode switch
        {
            StatusCodes.Status404NotFound => ApiErrorMessages.ResourceNotFound,
            StatusCodes.Status405MethodNotAllowed => ApiErrorMessages.MethodNotAllowed,
            StatusCodes.Status415UnsupportedMediaType => ApiErrorMessages.UnsupportedMediaType,
            >= StatusCodes.Status500InternalServerError => ApiErrorMessages.UnexpectedError,
            _ => ApiErrorMessages.InvalidRequest
        };

        private static Task WriteErrorAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = null;
            context.Response.ContentLength = null;
            return context.Response.WriteAsJsonAsync(new ErrorViewModel(statusCode, message), context.RequestAborted);
        }
    }
}
