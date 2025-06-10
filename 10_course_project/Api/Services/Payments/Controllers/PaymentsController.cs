using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Payments.Service.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(PaymentDbContext dbContext) : ControllerBase
{

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsForCustomer(Guid customerId)
    {
        var payments = await dbContext.Payments
            .Where(p => p.OrderId == customerId)
            .ToListAsync();

        if (payments == null || !payments.Any())
        {
            return NotFound();
        }

        return payments;
    }
}