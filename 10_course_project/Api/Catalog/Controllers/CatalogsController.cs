using Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Service.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogsController(IPublishEndpoint publishEndpoint, CatalogDbContext dbContext) : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> AddCatalogItem([FromBody] CreateCatalogItemRequest request)
        {
            var catalogItem = new CatalogItem
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Category = request.Category,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.CatalogItems.Add(catalogItem);
            await dbContext.SaveChangesAsync();

            // Publish event for CatalogItemAdded
            await publishEndpoint.Publish(new CatalogItemAdded(
                catalogItem.Id,
                catalogItem.Name,
                catalogItem.Description,
                catalogItem.Price));

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
    }

    public record CreateCatalogItemRequest(string Name, string Description, decimal Price, int StockQuantity, string Category);
}
