using Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Cart.Service.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController(IPublishEndpoint publishEndpoint, CartDbContext dbContext, IHttpClientFactory httpClientFactory) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> AddItem([FromBody] AddToCartRequest request)
    {
        var validationResult = ValidateCustomerId(request.CustomerId);
        if (validationResult is not OkResult)
        {
            return validationResult;
        }

        // Validate quantity
        if (request.Quantity < 0)
        {
            return BadRequest("Negative quantity not allowed");
        }

        var httpClient = httpClientFactory.CreateClient("Catalog");
        var catalogItemResponse = await httpClient.GetAsync($"api/catalog/{request.ProductId}");
        if (!catalogItemResponse.IsSuccessStatusCode)
        {
            return BadRequest("Product not found in catalog");
        }

        var catalogItem = await catalogItemResponse.Content.ReadFromJsonAsync<CatalogItem>();
        if (catalogItem == null)
        {
            return BadRequest("Invalid product data from catalog");
        }

        // Проверяю есть ли в наличии
        if (request.Quantity > catalogItem.StockQuantity)
        {
            return Conflict($"Not enough items in stock: {catalogItem.StockQuantity}");
        }
        
        // Проверяю что уже есть в корзине
        var existingItem = await dbContext.CartItems
            .FirstOrDefaultAsync(c =>
                c.CustomerId == request.CustomerId &&
                c.ProductId == request.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity = request.Quantity;
            await dbContext.SaveChangesAsync();
            return Ok(existingItem);
        }

        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Price = catalogItem.Price,
            ProductName = catalogItem.Name,
            ImageUrl = catalogItem.ImageUrl,
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

        return CreatedAtAction(nameof(AddItem), new { id = cartItem.Id });
    }

    [HttpDelete("{customerId}/{id}")]
    public async Task<IActionResult> RemoveItem(Guid customerId, Guid id)
    {
        var validationResult = ValidateCustomerId(customerId);
        if (validationResult is not OkResult)
        {
            return validationResult;
        }
        var cartItem = await dbContext.CartItems
            .FirstOrDefaultAsync(i => i.CustomerId == customerId && i.Id == id);
        if (cartItem == null)
        {
            return NotFound();
        }

        dbContext.Remove(cartItem);
        await dbContext.SaveChangesAsync();

        return Ok();
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

public record AddToCartRequest(Guid CustomerId, Guid ProductId, int Quantity);

public record CatalogItem(Guid Id, string Name, string Description, decimal Price, int StockQuantity, string Category, string Article, string ImageUrl);