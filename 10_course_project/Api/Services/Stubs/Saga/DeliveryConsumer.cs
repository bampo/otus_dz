using Common;
using MassTransit;
using Stubs.Service.DbContexts;
using Stubs.Service.Models;

namespace Stubs.Service.Saga;

public class DeliveryConsumer(StubsDbContext dbContext) : IConsumer<ReserveDelivery>
{

    public async Task Consume(ConsumeContext<ReserveDelivery> context)
    {
        // Симуляция резервирования курьера
        var courierAvailable = SimulateCourierAvailability(context.Message.TimeSlot);

        var delivery = new Delivery
        {
            Id = Guid.NewGuid(),
            OrderId = context.Message.OrderId,
            TimeSlot = context.Message.TimeSlot,
            CourierId = courierAvailable ? "Courier1" : null,
            Status = courierAvailable ? DeliveryStatus.Reserved : DeliveryStatus.Cancelled
        };

        dbContext.Deliveries.Add(delivery);
        await dbContext.SaveChangesAsync();

        if (courierAvailable)
            await context.Publish(new DeliveryReserved(context.Message.OrderId));
        else
            await context.Publish(new DeliveryCancelled(context.Message.OrderId, "No free couriers for slot"));
    }

    private bool SimulateCourierAvailability(int timeSlot) => timeSlot % 2 == 0 ; // Успешно, если в четные слоты
}