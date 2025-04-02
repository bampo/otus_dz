using Microsoft.Extensions.Configuration;
using Npgsql;
using UserProfile.Api.Extensions;
using Users.Dal;
using Users.Dal.Entities;

namespace UserProfile.Tests;

public class UserDalTests
{
    private const string CONN_STRING = "Host=srv1.home;Database=otus_dz;Username=test;";
    private IConfiguration Conf;
    private readonly NpgsqlConnectionStringBuilder csBuilder = new(CONN_STRING);
    public UserDalTests()
    {
        Conf = new ConfigurationBuilder()
            .AddUserSecrets<UserDalTests>()
            .Build();


        if (string.IsNullOrWhiteSpace(csBuilder.Password))
        {
            csBuilder.Password = Conf["DB_PASSWORD"] ?? throw new ArgumentException("Empty DB_PASSWORD");
        }        
        
    }

    [Fact]
    public void Test1()
    {
        var ctx = new UsersDbContext(csBuilder.ConnectionString);
        var u1 = new User
        {
            Email = "u1@test.mail", PasswordHash = Utils.CreatePwdHash("1")
            , ProfileInfo = new ProfileInfo()
            {
                Age = 20, Phone = "123123123", AvatarUri = "http://xzy.abc", FirstName = "User1First", LastName = "User1Last",
            }

        };

        ctx.Users.Add(u1);
        ctx.SaveChanges();

        var us = ctx.Users.ToList();
        Assert.NotEmpty(us);
    }
}