using MassTransit;
using Cart.Service;
using Common;
using Scalar.AspNetCore;
using Common.Helpers;

var builder = WebApplication.CreateBuilder(args);

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{env}.json", optional: true);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<CartDbContext>("cartdb");

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
                cfg.UseInMemoryOutbox(context);
                cfg.ConfigureEndpoints(context);
            });
    });


builder.Services.AddControllers();
builder.Services.AddOpenApi();

var servicesSecton = builder.Configuration.GetSection("Services").Get<ServicesConfigSection>()
    ?? throw new InvalidOperationException("No Services section in config");

builder.Services.AddHttpClient("Catalog", client =>
{
    client.BaseAddress = new Uri(servicesSecton.Catalog);
});

var app = builder.Build();

DbInit();

app.MapDefaultEndpoints();
app.MapScalarApiReference();
app.MapOpenApi();

app.UseMiddleware<UserIdMiddleware>();

app.MapControllers();

app.Run();

void DbInit()
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<CartDbContext>();
    ctx.Database.EnsureCreated();
}

public partial class Program { }