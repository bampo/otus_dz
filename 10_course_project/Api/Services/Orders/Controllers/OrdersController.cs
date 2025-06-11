using Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Service.Models;

namespace Orders.Service.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IPublishEndpoint publishEndpoint, OrderDbContext dbContext, IHttpClientFactory httpFactory) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var validationResult = ValidateCustomerId(request.CustomerId);
        if (validationResult is not OkResult)
        {
            return validationResult;
        }
        Guid orderListId;
        try
        {
            orderListId = await CreateOrderList(request);
        }
        catch (Exception e)
        {
            return Problem("Server error", statusCode: 500);
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            OrderListId = orderListId,
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
                order.OrderListId,
                order.TimeSlot
            ));

        return CreatedAtAction(nameof(CreateOrder), new { id = order.Id });
    }

    private async Task<Guid> CreateOrderList(CreateOrderRequest order)
    {
        var cart = httpFactory.CreateClient("Cart");
        cart.DefaultRequestHeaders.Add("X-User-Id", HttpContext.Items["UserId"]?.ToString());
        var items = await cart.GetFromJsonAsync<CartItem[]>($"/api/cart/{order.CustomerId}");
        var orderList = new OrderList()
        {
            CustomerId = order.CustomerId,
            OrderItems = items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList()
        };
        dbContext.OrdersLists.Add(orderList);
        await dbContext.SaveChangesAsync();

        return orderList.Id;
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

    private IActionResult ValidateCustomerId(Guid customerId)
    {
        var userIdString = HttpContext.Items["UserId"]?.ToString();

        if (!Guid.TryParse(userIdString, out var userId))
        {
            return BadRequest("Invalid user ID format");
        }

        if (!Guid.Equals(customerId, userId))
        {
            return Unauthorized();
        }

        return Ok(); 
    }
}

public record CreateOrderRequest(Guid CustomerId, int TimeSlot);
public record CartItem(Guid CustomerId, Guid ProductId, int Quantity, decimal Price);