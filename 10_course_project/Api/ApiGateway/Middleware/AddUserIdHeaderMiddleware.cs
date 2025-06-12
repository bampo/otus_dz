namespace ApiGateway.Middleware;

public class AddUserIdHeaderMiddleware(RequestDelegate next)
{

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var userId = context.User.FindFirst("sub")?.Value ?? context.User.FindFirst("user_id")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                context.Request.Headers["X-User-Id"] = userId;
            }
        }

        await next(context);
    }
}