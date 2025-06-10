using Customers;
using MassTransit;
using Scalar.AspNetCore;
using Customers.Services;
using Common.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<CustomerDbContext>("customerdb");

builder.Services.AddMassTransit(
    x =>
    {
        x.SetKebabCaseEndpointNameFormatter();
        x.AddConsumers(typeof(Program).Assembly);

        x.UsingRabbitMq(
            (context, cfg) =>
            {
                var configuration = context.GetRequiredService<IConfiguration>();
                var host = configuration.GetConnectionString("rabbitmq");
                cfg.Host(host);
                cfg.ConfigureEndpoints(context);
            });
    });

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register TokenProvider
builder.Services.AddSingleton<TokenProvider>();


var app = builder.Build();

DbInit();

app.MapDefaultEndpoints();
app.MapScalarApiReference();
app.MapOpenApi();

app.UseMiddleware<UserIdMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();
app.Run();

void DbInit()
{
    var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
    ctx.Database.EnsureCreated();
}

public partial class Program { }