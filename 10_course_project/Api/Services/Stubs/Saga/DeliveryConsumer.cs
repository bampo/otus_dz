using Common;
using MassTransit;
using Stubs.Service.DbContexts;
using Stubs.Service.Models;

namespace Stubs.Service.Saga;

public class DeliveryConsumer(StubsDbContext dbContext, ILogger<DeliveryConsumer> logger) : IConsumer<ReserveDelivery>
{

    public async Task Consume(ConsumeContext<ReserveDelivery> context)
    {
        logger.LogInformation("ReserveDelivery message received.");
        // Симуляция резервирования курьера
        var slot = context.Message.TimeSlot;
        var courierAvailable = SimulateCourierAvailability(slot);
        
        var delivery = new Delivery
        {
            Id = Guid.NewGuid(),
            OrderId = context.Message.OrderId,
            TimeSlot = slot,
            CourierId = courierAvailable ? "Courier1" : null,
            Status = courierAvailable ? DeliveryStatus.Reserved : DeliveryStatus.Cancelled
        };

        dbContext.Deliveries.Add(delivery);
        await dbContext.SaveChangesAsync();

        if (courierAvailable)
        {
            logger.LogInformation("Delivery reserved.");
            await context.Publish(new DeliveryReserved(context.Message.OrderId));
        }
        else
        {
            logger.LogWarning("Delivery falied. No couriers for slot {slot}.", slot);
            await context.Publish(new DeliveryCancelled(context.Message.OrderId, $"No free couriers for slot {slot}"));
        }
    }

    private bool SimulateCourierAvailability(int timeSlot) => timeSlot % 2 == 0 ; // Успешно, если в четные слоты
}