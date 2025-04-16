using Order.Service.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddHttpClient("billing-service", client => { client.BaseAddress = new Uri("http://billing-service"); });

builder.Services.AddHttpClient("notify-service", client => { client.BaseAddress = new Uri("http://notify-service"); });

builder.Services.AddScoped<OrdersService>();


var app = builder.Build();

app.MapDefaultEndpoints();


app.MapPost(
    "/api/order/create",
    async (OrderCreateRequest req, OrdersService ordersService) =>
    {
        var resp = await ordersService.CreateOrder(req.userId, req.price);
        return resp is not null 
            ? Results.Ok($"Order created: {resp}") 
            : Results.UnprocessableEntity("Not enought funds on deposit");
    });

app.MapOpenApi();
app.MapScalarApiReference();

app.Run();

public record OrderCreateRequest(string userId, decimal price);