using Microsoft.OpenApi.Models;
using EfDal;
using UserProfile.Api.Extensions;
using UserProfile.Api.Models;
using UserProfile.Dal;


var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//Применяю миграции, если задан параметр "--db-create"
if (await DbHelper.InitDb(args, builder)) return;


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
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
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service v1");
});

app.UseHttpsRedirection();

MapApiEndpoints(app);

app.Run();
return;


void MapApiEndpoints(WebApplication webApplication)
{
    webApplication.MapPost("/user", (UserDto user, UserRepository userRepository) =>
    {
        var dbUser = new User();
        user.CopyUser(dbUser);
        userRepository.AddUser(dbUser);
        return Results.Created($"/user/{dbUser.Id}", dbUser);
    });

    webApplication.MapGet("/user/{userId}", (int userId, UserRepository userRepository) =>
    {
        var user = userRepository.GetAllUsers().FirstOrDefault(u => u.Id == userId);
        return user != null ? Results.Ok(user) : Results.NotFound();
    });

    webApplication.MapDelete("/user/{userId}", (int userId, UserRepository userRepository) =>
    {
        userRepository.DeleteUser(userId);
        return Results.Ok($"Deleted Id: {userId}");
    });

    webApplication.MapPut("/user/{userId}", (int userId, UserDto user, UserRepository userRepository) =>
    {
        var existingUser = userRepository.GetAllUsers().FirstOrDefault(u => u.Id == userId);
    
        if (existingUser == null) return Results.NotFound();
    
        user.CopyUser(existingUser);
        userRepository.UpdateUser(existingUser);
        return Results.Ok(existingUser);
    });
}


