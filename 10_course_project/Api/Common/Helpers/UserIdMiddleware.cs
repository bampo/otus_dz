using Microsoft.AspNetCore.Http;

namespace Common.Helpers;

public class UserIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
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
}
