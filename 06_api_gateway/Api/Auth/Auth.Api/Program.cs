using Auth.Dal;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPost(
        "/login",
        (string username, string password, UserService userService) =>
        {
            var token = userService.Authenticate(username, password);
            return token == null ? Results.Unauthorized() : Results.Ok(new { token });
        })
    .WithOpenApi();

app.MapGet(
    "/auth",
    [Authorize]() => Results.Ok());

app.MapGet("/signin", () => "Get token on /login");

app.Run();