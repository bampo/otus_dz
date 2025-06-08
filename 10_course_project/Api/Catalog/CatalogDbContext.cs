using Microsoft.EntityFrameworkCore;

namespace Catalog.Service
{
    public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
    {

        public DbSet<CatalogItem> CatalogItems { get; set; }
    }

    public class CatalogItem
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public required string Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public required string Article { get; set; }
    }
}
