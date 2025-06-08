using Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cart.Service.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController(IPublishEndpoint publishEndpoint, CartDbContext dbContext) : ControllerBase
{
    private IActionResult ValidateCustomerId(Guid customerId)
    {
        var userIdString = HttpContext.Items["UserId"]?.ToString();

        if (!Guid.TryParse(userIdString, out var userId))
        {
            return BadRequest("Invalid user ID format");
        }

        if (!Guid.Equals(customerId, userId))
        {
            return StatusCode(403);
        }

        return Ok(); // Validation successful
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var validationResult = ValidateCustomerId(request.CustomerId);
        if (validationResult is not OkResult)
        {
            return validationResult;
        }

        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.CartItems.Add(cartItem);
        await dbContext.SaveChangesAsync();

        // Publish event for CartAdded
        await publishEndpoint.Publish(new CartAdded(
            cartItem.Id,
            cartItem.CustomerId,
            cartItem.ProductId,
            cartItem.Quantity,
            cartItem.Price));

        return CreatedAtAction(nameof(AddToCart), new { id = cartItem.Id });
    }

    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetCart([FromRoute] Guid customerId)
    {
        var validationResult = ValidateCustomerId(customerId);
        if (validationResult is not OkResult)
        {
            return validationResult;
        }

        var cartItems = await dbContext.CartItems
            .Where(c => c.CustomerId == customerId)
            .ToListAsync();

        return Ok(cartItems);
    }
}

public record AddToCartRequest(Guid CustomerId, Guid ProductId, int Quantity, decimal Price);