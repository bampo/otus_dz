using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Common.Helpers;

public class UserIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the method has [Anonymous] attribute
        var endpoint = context.GetEndpoint();
        if (endpoint != null &&
            (endpoint.Metadata.Any(m => m is AllowAnonymousAttribute) || DocsPath(endpoint)))
        {
            await next(context);
            return;
        }

        // Извлекаем X-User-Id
        if (context.Request.Headers.TryGetValue("X-User-Id", out var userId))
        {
            context.Items["UserId"] = userId;
            await next(context);
            return;
        }

        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Bad UserId");
    }

    private bool DocsPath(Endpoint endpoint)
        => endpoint is RouteEndpoint ep
           && (ep.RoutePattern.RawText.StartsWith("/scalar") || ep.RoutePattern.RawText.StartsWith("/openapi") 
);
}
