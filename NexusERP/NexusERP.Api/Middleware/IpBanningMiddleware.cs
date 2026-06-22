using Microsoft.Extensions.Caching.Memory;

namespace NexusERP.Api.Middleware
{
    public class IpBanningMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        public IpBanningMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";

            if (_cache.TryGetValue($"Banned_{ip}", out _))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"message\": \"Your IP has been temporarly banned due to suspicious activity.\"}");
                return;
            }

            await _next(context);
        }
    }
}
