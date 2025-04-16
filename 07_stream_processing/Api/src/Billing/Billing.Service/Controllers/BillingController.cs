using Microsoft.AspNetCore.Mvc;
using Billing.Dal;
using Billing.Dal.Models;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly BillngDbContext _context;

    public BillingController(BillngDbContext context) 
    {
        _context = context;
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req)
    {
        if (_context.Users.Any(u => u.Id == req.userId))
        {
            return BadRequest("User already exists.");
        }

        var user = new User { Id = req.userId, Balance = 0 };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { UserId = user.Id });
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] SetDepositRequest req)
    {
        var user = await _context.Users.FindAsync(req.userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }
        var newAmount = user.Balance + req.amount;
        if (newAmount < 0)
        {
            return UnprocessableEntity("InsufficientFunds");
        }
        
        user.Balance = newAmount;
        await _context.SaveChangesAsync();
        return Ok(new { UserId = user.Id, NewBalance = user.Balance });
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance([FromQuery] string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(new { UserId = user.Id, Balance = user.Balance });
    }
}
public record CreateUserRequest(string userId);
public record SetDepositRequest(string userId, decimal amount);