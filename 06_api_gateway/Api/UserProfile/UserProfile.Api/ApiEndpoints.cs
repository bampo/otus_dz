using System.ComponentModel.DataAnnotations;
using AutoMapper;
using UserProfile.Api.Extensions;
using UserProfile.Api.Models;
using Users.Dal;
using Users.Dal.Entities;

public partial class Program
{
    static void MapApiEndpoints(WebApplication webApp, ILogger<Program> log)
    {
        webApp.MapPost(
            "/register",
            async (RegisterUserDto user, UsersRepository repo, IMapper map) =>
            {
                log.LogInformation("Registering user '{email}'", user.Email);   
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(user);
                if (!Validator.TryValidateObject(user, validationContext, validationResults, true))
                {
                    return Results.ValidationProblem(
                        validationResults.ToDictionary(
                            v => v.MemberNames.FirstOrDefault() ?? "Error",
                            v => new[] { v.ErrorMessage! }
                        ));
                }

                var userExists = (await repo.GetAllUsers()).Any(u => u.Email.Equals(user.Email));
                if (userExists)
                {
                    var res = new ValidationResult($"Email '{user.Email}' already registered");
                    validationResults.Add(res);
                    log.LogWarning("User '{email}' already exists", user.Email);
                    return
                        Results.ValidationProblem(
                            validationResults.ToDictionary(
                                v => v.MemberNames.FirstOrDefault() ?? "Error",
                                v => new[] { v.ErrorMessage! }
                            ));
                }
                var dbUser = map.Map<User>(user);

                dbUser.PasswordHash = Utils.CreatePwdHash(user.Password);
                await repo.AddUser(dbUser);
                log.LogInformation("User {id} was created", dbUser.Id);
                return Results.Created($"/profile/{dbUser.Id}", new { UserId = dbUser.Id });
            });

        webApp.MapGet(
            "/profile",
            async (UsersRepository userRepository, IMapper map, HttpContext context) =>
            {
                if (!GetUserId(context, out var userId))
                {
                    log.LogWarning("X-User-Id was not profilded");
                    return Results.Unauthorized();
                }

                var profile = await userRepository.GetUserProfile(userId);
                
                if (profile is null) 
                    return Results.NotFound("User not exists");

                var dto = map.Map<ProfileDto>(profile);
                return Results.Ok(dto);
            });

        webApp.MapPut(
            "/profile",
            async (ProfileDto profileDto, UsersRepository userRepository, IMapper map, HttpContext context) =>
            {
                if (!GetUserId(context, out var userId))
                {
                    log.LogWarning("X-User-Id was not profilded");
                    return Results.Unauthorized();
                }
                if (profileDto.UserId != userId)
                {
                    log.LogWarning("X-User-Id != userId from request");
                    return Results.Unauthorized();
                }

                var profile = await userRepository.GetUserProfile(userId);
                if (profile is null)
                {
                    return Results.BadRequest("User not exists");
                }

                profile = map.Map(profileDto, profile);

                await userRepository.UpdateProfile(profile);
                var dto = map.Map<ProfileDto>(profileDto);
                return Results.Ok(dto);
            });
    }

    private static bool GetUserId(HttpContext httpContext, out int id)
    {
        int.TryParse(httpContext.Request.Headers["X-User-Id"], out var xid);
        id = xid;
        return id > 0;
    }
}