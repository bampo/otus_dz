using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentDbContext _dbContext;

    public PaymentsController(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsForCustomer(Guid customerId)
    {
        var payments = await _dbContext.Payments
            .Where(p => p.OrderId == customerId)
            .ToListAsync();

        if (payments == null || !payments.Any())
        {
            return NotFound();
        }

        return payments;
    }
}