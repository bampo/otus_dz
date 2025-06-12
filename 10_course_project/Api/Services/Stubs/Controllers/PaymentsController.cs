using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stubs.Service.DbContexts;
using Stubs.Service.Models;

namespace Stubs.Service.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(StubsDbContext dbContext) : ControllerBase
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