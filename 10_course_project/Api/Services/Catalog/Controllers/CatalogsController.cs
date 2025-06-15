using Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Service.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogsController(IPublishEndpoint publishEndpoint, CatalogDbContext dbContext) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> AddCatalogItem([FromBody] CreateCatalogItemRequest request)
    {
        // Check if item with same Article or Name already exists
        var existingItem = await dbContext.CatalogItems
            .FirstOrDefaultAsync(c => c.Article == request.Article || c.Name == request.Name);

        if (existingItem != null)
        {
            if (existingItem.Article == request.Article)
            {
                return Conflict($"Catalog item with Article {request.Article} already exists");
            }
            return Conflict($"Catalog item with Name {request.Name} already exists");
        }

        var catalogItem = new CatalogItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Category = request.Category,
            CreatedAt = DateTime.UtcNow,
            Article = request.Article,
            ImageUrl = request.ImageUrl
        };

        dbContext.CatalogItems.Add(catalogItem);
        await dbContext.SaveChangesAsync();


        // Publish event for CatalogItemAdded
        await publishEndpoint.Publish(
            new CatalogItemAdded(
                catalogItem.Id,
                catalogItem.Name,
                catalogItem.Description,
                catalogItem.Price,
                catalogItem.Article));

        return CreatedAtAction(nameof(AddCatalogItem), new { id = catalogItem.Id }, catalogItem);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllItems()
    {
        var items = await dbContext.CatalogItems.ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCatalogItem([FromRoute] Guid id)
    {
        var item = await dbContext.CatalogItems
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (item is null)
        {
            return NotFound($"Item with ID {id} was not found");
        }

        return Ok(item);
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetCatalogItems()
    {
        var items = await dbContext.CatalogItems.ToListAsync();
        return Ok(items);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCatalogItem([FromRoute] Guid id, [FromBody] CreateCatalogItemRequest request)
    {
        var existingItem = await dbContext.CatalogItems.FindAsync(id);
        if (existingItem == null)
        {
            return NotFound($"Item with ID {id} not found");
        }


        // Check if another item has same Article or Name
        var duplicateItem = await dbContext.CatalogItems
            .FirstOrDefaultAsync(
                c =>
                    (c.Article == request.Article || c.Name == request.Name) &&
                    c.Id != id);

        if (duplicateItem != null)
        {
            if (duplicateItem.Article == request.Article)
            {
                return Conflict($"Another catalog item with Article {request.Article} already exists");
            }
            return Conflict($"Another catalog item with Name {request.Name} already exists");
        }

        existingItem.Name = request.Name;
        existingItem.Description = request.Description;
        existingItem.Price = request.Price;
        existingItem.StockQuantity = request.StockQuantity;
        existingItem.Category = request.Category;
        existingItem.Article = request.Article;
        existingItem.ImageUrl = request.ImageUrl;

        await dbContext.SaveChangesAsync();

        return Ok(existingItem);
    }
}

public record CreateCatalogItemRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category,
    string Article,
    string ImageUrl);