using System.Net.Http.Json;
using Catalog.Service;
using Catalog.Service.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CatalogIntegrationTests
{
    public class CatalogsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public CatalogsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Configure in-memory database for testing
                    var serviceProvider = new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider();

                    services.AddDbContextPool<CatalogDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("CatalogTestDb");
                        options.UseInternalServiceProvider(serviceProvider);
                    });
                });
            });
        }

        protected async Task<CatalogDbContext> GetDbContext()
        {
            var scope = _factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CatalogDbContext>();

            // Ensure the database is clean before each test
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            return db;
        }

        [Fact]
        public async Task GetAllItems_ReturnsEmptyList()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/catalog");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<List<CatalogItem>>();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddCatalogItem_ReturnsCreatedResponse()
        {
            // Arrange
            var client = _factory.CreateClient();
            var db = await GetDbContext();
            var request = new CreateCatalogItemRequest(
                "Test Item",
                "This is a test item",
                9.99m,
                100,
                "Test Category"
            );

            // Act
            var response = await client.PostAsJsonAsync("/api/catalog", request);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CatalogItem>();
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Price, result.Price);
            Assert.Equal(request.StockQuantity, result.StockQuantity);
            Assert.Equal(request.Category, result.Category);

            // Verify item was actually saved to database
            var dbItem = await db.CatalogItems.FindAsync(result.Id);
            Assert.NotNull(dbItem);
            Assert.Equal(request.Name, dbItem.Name);
            Assert.Equal(request.Description, dbItem.Description);
            Assert.Equal(request.Price, dbItem.Price);
            Assert.Equal(request.StockQuantity, dbItem.StockQuantity);
            Assert.Equal(request.Category, dbItem.Category);
        }

        [Fact]
        public async Task GetCatalogItem_ReturnsCorrectItem()
        {
            // Arrange
            var db = await GetDbContext();

            // Add test data
            var catalogItem = new CatalogItem
            {
                Id = Guid.NewGuid(),
                Name = "Test Item",
                Description = "This is a test item",
                Price = 9.99m,
                StockQuantity = 100,
                Category = "Test Category",
                CreatedAt = DateTime.UtcNow
            };

            db.CatalogItems.Add(catalogItem);
            await db.SaveChangesAsync();

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync($"/api/catalog/{catalogItem.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CatalogItem>();
            Assert.NotNull(result);
            Assert.Equal(catalogItem.Id, result.Id);
            Assert.Equal(catalogItem.Name, result.Name);
            Assert.Equal(catalogItem.Description, result.Description);
            Assert.Equal(catalogItem.Price, result.Price);
            Assert.Equal(catalogItem.StockQuantity, result.StockQuantity);
            Assert.Equal(catalogItem.Category, result.Category);
        }
    }
}