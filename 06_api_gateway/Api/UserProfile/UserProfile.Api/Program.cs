using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AutoMapper;
using Microsoft.OpenApi.Models;
using UserProfile.Api.Extensions;
using UserProfile.Api.Models;
using UserProfile.Dal;


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
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service v1"); });

app.UseHttpsRedirection();

MapApiEndpoints(app);

app.Run();
return;


void MapApiEndpoints(WebApplication webApp)
{
    webApp.MapPost(
        "/register",
        (RegisterUserDto user, UserRepository repo, IMapper map) =>
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(user);
            if (!Validator.TryValidateObject(user, validationContext, validationResults, true))
            {
                return Results.ValidationProblem(
                    validationResults.ToDictionary(
                        v => v.MemberNames.FirstOrDefault() ?? "Error",
                        v => new string[] { v.ErrorMessage! }
                    ));
            }

            var userExists = repo.GetAllUsers().Any(u => u.Email.Equals(user.Email));
            if (userExists)
            {
                var res = new ValidationResult($"Email '{user.Email}' already registered");
                validationResults.Add(res);
                return
                    Results.ValidationProblem(
                        validationResults.ToDictionary(
                            v => v.MemberNames.FirstOrDefault() ?? "Error",
                            v => new[] { v.ErrorMessage! }
                        ));
            }
            var dbUser = map.Map<User>(user);

            dbUser.PasswordHash = Utils.CreatePwdHash(user.Password);
            repo.AddUser(dbUser);
            return Results.Created($"/user/{dbUser.Id}", new { UserId = dbUser.Id });
        });

    webApp.MapGet(
        "/user/{id}",
        (int id, UserRepository userRepository, IMapper map, HttpContext context) =>
        {
            if (!ValidXUserId(context, id))
            {
                return Results.Unauthorized();
            }

            var user = userRepository.GetAllUsers().FirstOrDefault(u => u.Id == id);
            var dto = map.Map<RegisterUserDto>(user);
            return user != null ? Results.Ok(dto) : Results.NotFound();
        });

    webApp.MapDelete(
        "/user/{id}",
        (int id, UserRepository userRepository, HttpContext context) =>
        {
            if (!ValidXUserId(context, id))
            {
                return Results.Unauthorized();
            }
            userRepository.DeleteUser(id);
            return Results.Ok($"Deleted Id: {id}");
        });

    webApp.MapPut(
        "/user/{id}",
        (int id, UpdateUserDto user, UserRepository userRepository, IMapper map, HttpContext context) =>
        {
            if (!ValidXUserId(context, id))
            {
                return Results.Unauthorized();
            }

            var existingUser = userRepository.GetAllUsers().FirstOrDefault(u => u.Id == id);

            if (existingUser == null) return Results.NotFound();
            
            existingUser = map.Map(user, existingUser);
            
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existingUser.PasswordHash = Utils.CreatePwdHash(user.Password);
            }

            userRepository.UpdateUser(existingUser);
            var dto = map.Map<UpdateUserDto>(user);
            return Results.Ok(dto);
        });
}

bool ValidXUserId(HttpContext httpContext, int id)
{
    int.TryParse(httpContext.Request.Headers["X-User-Id"], out var xUserId);
    return xUserId != 0 && xUserId == id;
}