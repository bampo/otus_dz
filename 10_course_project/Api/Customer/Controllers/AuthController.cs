using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Customers.Models;
using Customers.Services;
using MassTransit;
using Common;

namespace Customers.Controllers
{
    [Route("api/customers/[controller]")]
    [ApiController]
    public class AuthController(CustomerDbContext context, TokenProvider tokenProvider, IPublishEndpoint publishEndpoint) : ControllerBase()
    {
    [HttpPost("register")]
        public async Task<ActionResult<int>> Register(RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Password is required");
            }

            var salt = PasswordHandler.GenerateSalt();
            var hashedPassword = PasswordHandler.HashPassword(model.Password, salt);

            var customer = new Customer
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = hashedPassword,
                Salt = salt
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            await publishEndpoint.Publish(new CustomerRegistered(
                customer.Id,
                customer.Email,
                customer.FirstName,
                customer.LastName));

            return Ok(customer.Id);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginModel model)
        {
            var customer = await context.Customers.FirstOrDefaultAsync(x => x.Email == model.Email);

            if (customer == null || !PasswordHandler.VerifyPassword(model.Password, customer.Salt, customer.PasswordHash))
            {
                return Unauthorized();
            }

            var customerId = new Guid(customer.Id.ToString());
            var token = tokenProvider.GenerateToken(customer.Email, customerId);
            return Ok(token);
        }
    }

    public class LoginModel
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
