using Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Service.Models;

namespace Orders.Service.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly OrderDbContext _dbContext;

    public OrdersController(IPublishEndpoint publishEndpoint, OrderDbContext dbContext)
    {
        _publishEndpoint = publishEndpoint;
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
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

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var cs = _dbContext.Database.GetConnectionString();

        await _publishEndpoint.Publish(new OrderCreated
        (
            order.Id,
            order.CustomerId,
            order.ProductId,
            order.Quantity,
            order.Amount,
            order.TimeSlot
        ));

        return CreatedAtAction(nameof(CreateOrder), new {id = order.Id});
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrdersById([FromRoute] Guid orderId)
    {
        var order = await _dbContext.Orders
            .Where(o => o.Id == orderId)
            .FirstOrDefaultAsync();

        if (order is null)
        {
            return NotFound($"Order {orderId} was not found");
        }
        return Ok(order);
    }
}

public record CreateOrderRequest(Guid CustomerId, Guid ProductId, int Quantity, decimal Amount, int TimeSlot);