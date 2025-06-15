using Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Service.Models;

namespace Orders.Service.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(IPublishEndpoint publishEndpoint, OrderDbContext dbContext, 
    IHttpClientFactory httpFactory, ILogger<OrdersController> logger
) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var validationResult = ValidateCustomerId(request.CustomerId);
        if (validationResult is not OkResult)
        {
            return validationResult;
        }

        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) || string.IsNullOrEmpty(idempotencyKey))
        {
            logger.LogWarning("No idempotency key was provided");
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

                    OrderList orderList;
                    try
                    {
                        
                        orderList = await CreateOrderList(request);
                        logger.LogInformation("Created OrderList: {listId}", orderList.Id);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error creating order list");
                        return Problem("Server error", statusCode: 500);
                    }

                    var order = new Order
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = request.CustomerId,
                        OrderListId = orderList.Id,
                        Status = "Pending",
                        TimeSlot = request.TimeSlot,
                        CreatedAt = DateTime.UtcNow,
                        Amount = orderList.Amount,
                        IdempotencyKey = idempotencyKey
                    };
                    await dbContext.Orders.AddAsync(order);
                    await dbContext.SaveChangesAsync();

                    logger.LogInformation("Created order {id}", order.Id);


                    await publishEndpoint.Publish(
                        new OrderCreated(
                            order.Id,
                            order.CustomerId,
                            order.OrderListId,
                            order.TimeSlot,
                            orderList.Amount
                        ));

                    await transaction.CommitAsync();

                    return CreatedAtAction(nameof(CreateOrder), order);
                }
                catch(Exception e)
                {
                    logger.LogError(e, "Error when create order");
                    await transaction.RollbackAsync();
                    throw;
                }
            });
    }

    private async Task<OrderList> CreateOrderList(CreateOrderRequest request)
    {
        var cart = httpFactory.CreateClient("Cart");
        cart.DefaultRequestHeaders.Add("X-User-Id", HttpContext.Items["UserId"]?.ToString());
        var items = await cart.GetFromJsonAsync<CartItem[]>($"/api/cart/{request.CustomerId}");
        var orderList = new OrderList()
        {
            CustomerId = request.CustomerId,
            OrderItems = items.Select(
                i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    ProductName = i.ProductName
                }).ToList(),
        };
        orderList.Amount = CalcAmount(orderList.OrderItems);
        dbContext.OrdersLists.Add(orderList);
        await dbContext.SaveChangesAsync();

        return orderList;
    }

    private static decimal CalcAmount(ICollection<OrderItem> items)
        => items.Sum(i => i.Price * i.Quantity);


    [HttpGet("{customerId}/{request}")]
    public async Task<IActionResult> GetOrdersById([FromRoute] Guid customerId,[FromRoute] Guid id)
    {
        var validationResult = ValidateCustomerId(customerId);
        if (validationResult is not OkResult)
        {
            return validationResult;
        }

        var order = await dbContext.Orders
            .Where(o => o.Id == id && o.CustomerId == customerId)
            .FirstOrDefaultAsync();

        if (order is null)
        {
            return NotFound($"Order {id} was not found");
        }
        return Ok(order);
    }

    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetOrders([FromRoute] Guid customerId)
    {
        var ordersWithItems = await (from order in dbContext.Orders
            where order.CustomerId == customerId
            join orderList in dbContext.OrdersLists on order.OrderListId equals orderList.Id
            select new
            {
                Order = order,
                OrderItems = orderList.OrderItems
            }).OrderByDescending(o => o.Order.CreatedAt).ToListAsync();

        return Ok(ordersWithItems);
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

public record CartItem(Guid CustomerId, Guid ProductId, string ProductName, int Quantity, decimal Price);