using System.Net;
using System.Net.Http.Json;
using Cart.Service;
using Cart.Service.Controllers;
using Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CartIntegrationTests
{
    public class CartControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();

        public CartControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                });

                builder.ConfigureServices(services =>
                {
                    // Replace IPublishEndpoint with our mock
                    services.AddSingleton(_publishEndpointMock.Object);

                    // Configure in-memory database for testing
                    var serviceProvider = new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider();

                    services.AddDbContextPool<CartDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("CartTestDb");
                        options.UseInternalServiceProvider(serviceProvider);
                    });
                });
            });
        }

        protected async Task<CartDbContext> GetDbContext()
        {
            var scope = _factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CartDbContext>();

            // Ensure the database is clean before each test
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            return db;
        }

        [Fact]
        public async Task AddToCart_ReturnsCreated_WhenValidRequest()
        {
            // Arrange
            var db = await GetDbContext();
            var customerId = Guid.NewGuid();
            var request = new AddToCartRequest(customerId, Guid.NewGuid(), 2);

            // Act
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", customerId.ToString());
            var response = await client.PostAsJsonAsync("api/cart", request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Verify cart item was created
            var cartItem = await db.CartItems.FirstOrDefaultAsync();
            Assert.NotNull(cartItem);
            Assert.Equal(request.CustomerId, cartItem.CustomerId);
            Assert.Equal(request.ProductId, cartItem.ProductId);
            Assert.Equal(request.Quantity, cartItem.Quantity);
// Price is set by the controller based on catalog lookup
;

            // Verify event was published
            _publishEndpointMock.Verify(x => x.Publish(It.Is<CartAdded>(e =>
                e.CustomerId == customerId &&
                e.ProductId == request.ProductId &&
                e.Quantity == request.Quantity), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddToCart_ReturnsBadRequest_WhenInvalidUserId()
        {
            // Arrange
            await GetDbContext();
            var request = new AddToCartRequest(Guid.NewGuid(), Guid.NewGuid(), 1);

            // Act
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", "invalid-guid");
            var response = await client.PostAsJsonAsync("/api/cart", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid user ID", content);
        }

        [Fact]
        public async Task AddToCart_ReturnsForbidden_WhenUserIdMismatch()
        {
            // Arrange
            await GetDbContext();
            var request = new AddToCartRequest(Guid.NewGuid(), Guid.NewGuid(), 1);

            // Act
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", Guid.NewGuid().ToString());
            var response = await client.PostAsJsonAsync("/api/cart", request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("User ID mismatch", content);
        }

        [Fact]
        public async Task GetCart_ReturnsCartItems_WhenValidRequest()
        {
            // Arrange
            var db = await GetDbContext();
            var customerId = Guid.NewGuid();

            // Add test data
            var cartItem = new CartItem
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                ProductId = Guid.NewGuid(),
                Quantity = 2,
                Price = 10.99m,
                CreatedAt = DateTime.UtcNow
            };
            db.CartItems.Add(cartItem);
            await db.SaveChangesAsync();

            // Act
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", customerId.ToString());
            var response = await client.GetAsync($"/api/cart/{customerId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CartItem[]>();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(cartItem.Id, result[0].Id);
            Assert.Equal(cartItem.ProductId, result[0].ProductId);
            Assert.Equal(cartItem.Quantity, result[0].Quantity);
            Assert.Equal(cartItem.Price, result[0].Price);
        }

        [Fact]
        public async Task GetCart_ReturnsEmptyArray_WhenNoItems()
        {
            // Arrange
            await GetDbContext();
            var customerId = Guid.NewGuid();

            // Act
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-User-Id", customerId.ToString());
            var response = await client.GetAsync($"/api/cart/{customerId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<CartItem[]>();
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}