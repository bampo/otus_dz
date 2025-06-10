using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Customers.Models;
using Customers.Services;
using MassTransit;
using Common;
using Microsoft.AspNetCore.Authorization;

namespace Customers.Controllers
{
    [Route("api/customers/[controller]")]
    [ApiController]
    public class AuthController(CustomerDbContext context, TokenProvider tokenProvider, IPublishEndpoint publishEndpoint) : ControllerBase()
    {

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Password is required");
            }
            if (context.Customers.Any(c => c.Email.ToLower() == model.Email.ToLower()))
            {
                return BadRequest("User already registered");
            }

            var salt = PasswordHandler.GenerateSalt();
            var hashedPassword = PasswordHandler.HashPassword(model.Password, salt);

            var customer = new Customer
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PasswordHash = hashedPassword,
                Salt = salt,
                Active = false
            };

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var customerRegistered = new CustomerRegistered(
                customer.Id,
                customer.Email,
                customer.FirstName,
                customer.LastName);

            await publishEndpoint.Publish(customerRegistered);

            return Ok(customerRegistered);
        }

        [HttpGet("confirm-email/{customerId}")]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(Guid customerId)
        {
            var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id ==  customerId && !c.Active);
            if (customer == null)
            {
                return NotFound();
            }
    
            customer.Active = true;
            await context.SaveChangesAsync();
            
            return Ok("Email confirmed successfully");
        }
    
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginRequest request)
        {
            var customer = await context.Customers.FirstOrDefaultAsync(x => x.Email == request.Email);
    
            if (customer == null || !customer.Active || !PasswordHandler.VerifyPassword(request.Password, customer.Salt, customer.PasswordHash))
            {
                return Unauthorized();
            }

            var customerId = new Guid(customer.Id.ToString());
            var token = tokenProvider.GenerateToken(customer.Email, customerId);
            return Ok(new LoginResponse(token));
        }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
    public record LoginResponse(string Token);
}