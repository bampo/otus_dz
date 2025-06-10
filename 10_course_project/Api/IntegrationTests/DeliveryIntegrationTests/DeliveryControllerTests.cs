using Common;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Json;
using Xunit;
using Delivery.Service.Models;
using Delivery.Service;

namespace DeliveryIntegrationTests;

public record ReserveDelivery(Guid OrderId, int TimeSlot);
public record DeliveryReserved(Guid OrderId);
public record DeliveryReservationFailed(Guid OrderId, string Reason);

public class DeliveryControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();

    public DeliveryControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(_publishEndpointMock.Object);

                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContextPool<DeliveryDbContext>(options =>
                {
                    options.UseInMemoryDatabase("DeliveryTestDb");
                    options.UseInternalServiceProvider(serviceProvider);
                });
            });
        });
    }

    protected async Task<DeliveryDbContext> GetDbContext()
    {
        var scope = _factory.Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<DeliveryDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        return db;
    }

    [Fact]
    public async Task GetDeliverySlots_ReturnsEmptyList()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/delivery/slots");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<DeliverySlot>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ReserveDelivery_WithAvailableSlot_ReturnsOk()
    {
        var db = await GetDbContext();
        var slot = new DeliverySlot { TimeSlot = (int)DateTime.UtcNow.AddDays(1).Ticks, IsAvailable = true };
        db.DeliverySlots.Add(slot);
        await db.SaveChangesAsync();

        var client = _factory.CreateClient();
       // Use a valid reservation request
       var request = new ReserveDelivery(Guid.NewGuid(), slot.TimeSlot);

        var response = await client.PostAsJsonAsync("/api/delivery/reserve", request);
        response.EnsureSuccessStatusCode();

        var updatedSlot = await db.DeliverySlots.FindAsync(slot.Id);
        Assert.False(updatedSlot.IsAvailable);

        var reservation = await db.DeliveryReservations.FirstOrDefaultAsync();
        Assert.NotNull(reservation);
        Assert.Equal(request.OrderId, reservation.OrderId);
        Assert.Equal(request.TimeSlot, reservation.TimeSlot);

        _publishEndpointMock.Verify(x => x.Publish(It.Is<DeliveryReserved>(e => e.OrderId == request.OrderId), default), Times.Once);
    }

    [Fact]
    public async Task ReserveDelivery_WithNoAvailableSlots_ReturnsBadRequest()
    {
        var db = await GetDbContext();
        var slot = new DeliverySlot { TimeSlot = (int)DateTime.UtcNow.AddDays(1).Ticks, IsAvailable = false };
        db.DeliverySlots.Add(slot);
        await db.SaveChangesAsync();

        var client = _factory.CreateClient();
        var request = new ReserveDelivery(Guid.NewGuid(), slot.TimeSlot);

        var response = await client.PostAsJsonAsync("/api/delivery/reserve", request);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("No available slots", content);

        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<DeliveryReservationFailed>(e =>
                e.OrderId == request.OrderId &&
                e.Reason == "Time slots unavailable"),
            default), Times.Once);
    }

    [Fact]
    public async Task CancelDelivery_WithExistingReservation_ReturnsOk()
    {
        var db = await GetDbContext();
        var slot = new DeliverySlot { TimeSlot = (int)DateTime.UtcNow.AddDays(1).Ticks, IsAvailable = false };
        var reservation = new DeliveryReservation { OrderId = Guid.NewGuid(), TimeSlot = slot.TimeSlot };
        db.DeliverySlots.Add(slot);
        db.DeliveryReservations.Add(reservation);
        await db.SaveChangesAsync();

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/delivery/cancel", reservation);

        response.EnsureSuccessStatusCode();
        var updatedSlot = await db.DeliverySlots.FindAsync(slot.Id);
        Assert.True(updatedSlot.IsAvailable);

        var existingReservation = await db.DeliveryReservations.FindAsync(reservation.Id);
        Assert.Null(existingReservation);
    }

    [Fact]
    public async Task CancelDelivery_WithNonExistentReservation_ReturnsOk()
    {
        var db = await GetDbContext();
        var client = _factory.CreateClient();

        // Create a reservation that doesn't exist in the database
        var fakeReservation = new DeliveryReservation { OrderId = Guid.NewGuid(), TimeSlot = (int)DateTime.UtcNow.Ticks };

        // Verify the reservation doesn't exist
        var existingReservation = await db.DeliveryReservations.FirstOrDefaultAsync(r => r.OrderId == fakeReservation.OrderId);
        Assert.Null(existingReservation);

        // Attempt to cancel the non-existent reservation
        var response = await client.PostAsJsonAsync("/api/delivery/cancel", fakeReservation);
        response.EnsureSuccessStatusCode();

        // Verify the database state hasn't changed
        var reservationsCount = await db.DeliveryReservations.CountAsync();
        Assert.Equal(0, reservationsCount);
    }

    [Fact]
    public async Task ReserveDelivery_WithInvalidInput_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        // Test with invalid OrderId (empty GUID)
        var response1 = await client.PostAsJsonAsync("/api/delivery/reserve", new ReserveDelivery(Guid.Empty, (int)DateTime.UtcNow.AddDays(1).Ticks));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response1.StatusCode);
        var content1 = await response1.Content.ReadAsStringAsync();
        Assert.Contains("Invalid order ID", content1);

        // Test with invalid TimeSlot (in the past)
        var response2 = await client.PostAsJsonAsync("/api/delivery/reserve", new ReserveDelivery(Guid.NewGuid(), (int)DateTime.UtcNow.AddDays(-1).Ticks));
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response2.StatusCode);
        var content2 = await response2.Content.ReadAsStringAsync();
        Assert.Contains("Invalid time slot", content2);
    }

    [Fact]
    public async Task ReserveDelivery_WithConcurrentRequests_HandlesRaceCondition()
    {
        var db = await GetDbContext();
        var slot = new DeliverySlot { TimeSlot = (int)DateTime.UtcNow.AddDays(1).Ticks, IsAvailable = true };
        db.DeliverySlots.Add(slot);
        await db.SaveChangesAsync();

        var client = _factory.CreateClient();
        var request = new ReserveDelivery(Guid.NewGuid(), slot.TimeSlot);

        // Create concurrent requests
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 5; i++)
        {
            // Use a unique OrderId for each request to ensure proper tracking
            var uniqueRequest = new ReserveDelivery(Guid.NewGuid(), slot.TimeSlot);
            tasks.Add(client.PostAsJsonAsync("/api/delivery/reserve", uniqueRequest));
        }

        // Execute all requests concurrently
        var responses = await Task.WhenAll(tasks);

        // Verify only one request succeeded
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        Assert.Equal(1, successCount);

        // Verify only one reservation was created
        var reservations = await db.DeliveryReservations.CountAsync();
        Assert.Equal(1, reservations);

        // Verify the slot is no longer available
        var updatedSlot = await db.DeliverySlots.FindAsync(slot.Id);
        Assert.False(updatedSlot.IsAvailable);
    }
}