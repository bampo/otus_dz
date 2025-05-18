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

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {

        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) || string.IsNullOrEmpty(idempotencyKey))
        {
            return BadRequest("Idempotency-Key header is required.");
        }

        var dbStrategy = dbContext.Database.CreateExecutionStrategy();

        return await dbStrategy.ExecuteAsync(
            async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync();

                try
                {
                    var existingOrder = await dbContext.Orders
                        .Where(o => o.IdempotencyKey == (string)idempotencyKey)
                        .FirstOrDefaultAsync();

                    if (existingOrder != null)
                    {
                        return CreatedAtAction(nameof(CreateOrder), new { orderId = existingOrder.Id }, existingOrder);
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
                        CreatedAt = DateTime.UtcNow,
                        IdempotencyKey = idempotencyKey!
                    };

                    dbContext.Orders.Add(order);
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


                    await transaction.CommitAsync();

                    return CreatedAtAction(nameof(GetOrdersById), new { orderId = order.Id }, order);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
    }


    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrdersById([FromRoute] Guid orderId)
    {
        var order = await dbContext.Orders
            .Where(o => o.Id == orderId)
            .FirstOrDefaultAsync();

        if (order is null)
        {
            return NotFound($"Order {orderId} was not found");
        }
        return Ok(order);
    }
}

public record CreateOrderRequest(
    Guid CustomerId,
    Guid ProductId,
    int Quantity,
    decimal Amount,
    int TimeSlot);