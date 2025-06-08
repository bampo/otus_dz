using Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Service.Models;

namespace Orders.Service.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IPublishEndpoint publishEndpoint, OrderDbContext dbContext) : ControllerBase
{

    private IActionResult ValidateCustomerId(Guid customerId)
    {
        string userIdString = (string)HttpContext.Items["UserId"]!;

        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return BadRequest("Invalid user ID format");
        }

        if (!Guid.Equals(customerId, userId))
        {
            return StatusCode(403);
        }

        return null; // No error
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var validationResult = ValidateCustomerId(request.CustomerId);
        if (validationResult != null)
        {
            return validationResult;
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Amount = request.Amount,
            Status = "Pending",
            TimeSlot = request.TimeSlot,
            CreatedAt = DateTime.UtcNow
        };
        await dbContext.Orders.AddAsync(order);
        await dbContext.SaveChangesAsync();

        await publishEndpoint.Publish(
            new OrderCreated(
                order.Id,
                order.CustomerId,
                order.ProductId,
                order.Quantity,
                order.Amount,
                order.TimeSlot
            ));

        return CreatedAtAction(nameof(CreateOrder), new { id = order.Id });
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrdersById([FromRoute] Guid orderId)
    {
        string userIdString = (string)HttpContext.Items["UserId"]!;

        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return BadRequest("Invalid user ID format");
        }

        var order = await dbContext.Orders
            .Where(o => o.Id == orderId && o.CustomerId == userId)
            .FirstOrDefaultAsync();

        if (order is null)
        {
            return NotFound($"Order {orderId} was not found");
        }
        return Ok(order);
    }
}

public record CreateOrderRequest(Guid CustomerId, Guid ProductId, int Quantity, decimal Amount, int TimeSlot);