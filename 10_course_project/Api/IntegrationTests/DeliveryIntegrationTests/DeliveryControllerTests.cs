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
        var fakeReservation = new DeliveryReservation { OrderId = Guid.NewGuid(), TimeSlot = (int)DateTime.UtcNow.Ticks };

        var response = await client.PostAsJsonAsync("/api/delivery/cancel", fakeReservation);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ReserveDelivery_WithInvalidInput_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        var response1 = await client.PostAsJsonAsync("/api/delivery/reserve", new { OrderId = Guid.Empty, TimeSlot = (int)DateTime.UtcNow.AddDays(1).Ticks });
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response1.StatusCode);

        var response2 = await client.PostAsJsonAsync("/api/delivery/reserve", new { OrderId = Guid.NewGuid(), TimeSlot = (int)DateTime.UtcNow.AddDays(-1).Ticks });
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response2.StatusCode);
    }

    [Fact]
    public async Task ReserveDelivery_WithConcurrentRequests_HandlesRaceCondition()
    {
        var db = await GetDbContext();
        var slot = new DeliverySlot { TimeSlot = (int)DateTime.UtcNow.AddDays(1).Ticks, IsAvailable = true };
        db.DeliverySlots.Add(slot);
        await db.SaveChangesAsync();

        var client = _factory.CreateClient();
        var request = new { OrderId = Guid.NewGuid(), TimeSlot = slot.TimeSlot };

        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(client.PostAsJsonAsync("/api/delivery/reserve", request));
        }
        var responses = await Task.WhenAll(tasks);

        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        Assert.Equal(1, successCount);

        var reservations = await db.DeliveryReservations.CountAsync();
        Assert.Equal(1, reservations);
    }
}