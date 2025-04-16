using Microsoft.EntityFrameworkCore;
using Notify.Dal;
using Notify.Service;
using Notify.Service.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Configuration.AddUserSecrets<Program>();
InitDb(builder);

builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenApi();
app.MapScalarApiReference();


app.MapPost("/api/notify/notification", async (NotificationService service, NotificationRequest request) =>
{
    await service.AddNotificationAsync(request.userId, request.message);
    return Results.Ok();
});

app.MapGet("/api/notify/notifications", async (NotificationService service, string userId) =>
{
    var notifications = await service.GetNotificationsAsync();
    return Results.Ok(notifications.Where(n => n.UserId == userId));
});


app.Run();


void InitDb(WebApplicationBuilder builder)
{
    var password = builder.Configuration["DB_PASSWORD"];
    var constring = builder.Configuration.GetConnectionString("Default").Replace("{DB_PASSWORD}", password);

    builder.Services.AddDbContext<NotifyDbContext>(options => options.UseNpgsql(constring));
    using (var scope = builder.Services.BuildServiceProvider().CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<NotifyDbContext>();
        //context.Database.EnsureDeleted();        
        context.Database.EnsureCreated();
    }
}
