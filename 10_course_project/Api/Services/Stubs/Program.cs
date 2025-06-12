using Common.Helpers;
using MassTransit;
using Scalar.AspNetCore;
using Stubs.Service.DbContexts;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

builder.AddNpgsqlDbContext<StubsDbContext>("stubsdb");

var app = builder.Build();

DbInit();

app.MapDefaultEndpoints();
app.MapOpenApi();
app.MapScalarApiReference();

app.MapControllers();

app.Run();
return;

void DbInit()
{
    var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<StubsDbContext>();
    ctx.Database.EnsureCreated();
}

public partial class Program { }