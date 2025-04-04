using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using UserProfile.Api.Extensions;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


//Применяю миграции, если задан параметр "--db-create"
if (await DbHelper.InitDb(args, builder)) return;

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title = "User Service",
                Version = "1.0.0",
                Description = "WebAPI для ДЗ OTUS",
                Contact = new OpenApiContact { Email = "vpmail@gmail.com" }
            });
    });

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
var log = app.Services.GetRequiredService<ILogger<Program>>();
MapApiEndpoints(app, log);

log.LogInformation("APP STARTED");
app.Run();
return;


