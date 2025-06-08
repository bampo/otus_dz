using Common.Helpers;
using MassTransit;
using Orders.Service;
using Orders.Service.Saga;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddMassTransit(
    x =>
    {
        x.SetKebabCaseEndpointNameFormatter();
        x.AddConsumers(typeof(Program).Assembly);

        x.AddSagaStateMachine<OrderSaga, OrderSagaState>()
            .EntityFrameworkRepository(
                r =>
                {
                    r.ExistingDbContext<OrderDbContext>();
                    r.UsePostgres();
                });


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

builder.AddNpgsqlDbContext<OrderDbContext>("ordersdb");

var app = builder.Build();

DbInit();

app.MapDefaultEndpoints();
app.MapScalarApiReference();
app.MapOpenApi();

app.UseAuthorization();
app.UseMiddleware<UserIdMiddleware>();

app.MapControllers();

app.Run();
return;

void DbInit()
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    ctx.Database.EnsureCreated();
}