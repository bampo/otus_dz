using Auth.Dal;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var conStr = builder.Configuration.GetConnectionString("Default");
ArgumentException.ThrowIfNullOrEmpty(conStr);

builder.Services.AddSingleton(new ApplicationDbContext(conStr));

builder.Services.AddScoped<UserService>();

builder.Services.AddAuthorization();


// JWT Authentication
builder.Services.AddAuthentication(
        options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
    .AddJwtBearer(
        options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };
        });

var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPost(
        "/login",
        (LoginRequest req, UserService userService) =>
        {
            var token = userService.Authenticate(req.login, req.password);
            if (token == null)
            {
                app.Logger.LogWarning("Bad login attempt for login: '{login}'", req.login);
                return Results.Unauthorized();
            }

            app.Logger.LogInformation("Successful auth for login '{login}'", req.login);
            return Results.Ok(new { token });
        })
    .WithOpenApi();

app.MapGet(
    "/auth",
    [Authorize](HttpContext context) =>
    {
        var userId = context.User.FindFirst(ClaimTypes.Name)?.Value;

        if (userId is null)
        {
            app.Logger.LogWarning("UserId was not found");
            return Results.Unauthorized();
        }
        
        app.Logger.LogInformation("Authenticate UserId: {userId} from JWT token", userId);
        context.Response.Headers["X-User-Id"] = userId;
        return Results.Ok();
    });

app.MapGet("/signin", () => "Get token on /login");

app.Run();

public class LoginRequest
{
    public string login { get; set; }
    public string password { get; set; }
}