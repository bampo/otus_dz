﻿using Microsoft.EntityFrameworkCore;
using Npgsql;
using Users.Dal;

namespace UserProfile.Api.Extensions;

public static class DbHelper
{
    public static async Task<bool> InitDb(string[] args,  WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Default");
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Не найдена строка подключения в appsettings.json.");
            return true;
        }

        var csBuilder = new NpgsqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(csBuilder.Password))
        {
            csBuilder.Password = builder.Configuration["DB_PASSWORD"] ?? throw new ArgumentException("Empty DB_PASSWORD");
        }

        builder.Services.AddScoped(provider => new UsersDbContext(csBuilder.ConnectionString));
        builder.Services.AddScoped<UsersRepository>();

        var exitAfterMigrations = args.Contains("--db-create");
        
        using var scope = builder.Services.BuildServiceProvider().CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        try
        {
            var context = services.GetRequiredService<UsersDbContext>();

            //await WaitDb(context);
            //context.Database.EnsureDeleted();
            await context.Database.EnsureCreatedAsync();

            if (!(await context.Database.GetPendingMigrationsAsync()).Any())
            {
                logger.LogInformation("Migrations was already applied to DB");
                return exitAfterMigrations;
            }

            logger.LogInformation("Start DB migrations");
            await context.Database.MigrateAsync();
            logger.LogInformation("DB Migration finished");
        }
        catch (Exception ex)
        {

            logger.LogError(ex, "Error migrating DB");
        }


        return exitAfterMigrations;
    }

    /*private static async Task WaitDb(DbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        var retries = 5;
        while(!await db.Database.CanConnectAsync())
        {
            Console.WriteLine("Wait for DB ...");
            await Task.Delay(2000);
            if (--retries < 0) return;
            throw new TimeoutException("Can't connect to DB");
        }
    }*/
}