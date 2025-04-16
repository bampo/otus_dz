using Scalar.AspNetCore;
using User.Service;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddHttpClient<BillingService>(client =>
{
    client.BaseAddress = new Uri("http://billing-service");
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapPost("/api/user/create", async (UserCreateRequest req, BillingService billingService) =>
{
    var userId = Guid.NewGuid().ToString();
    var resp = await billingService.CreateUser(userId);
    return Results.Ok(new { UserId = userId });
});

app.Run();

public record UserCreateRequest(string email, string password);