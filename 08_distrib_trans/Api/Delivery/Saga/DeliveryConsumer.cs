using Common;
using Delivery.Service.Models;
using MassTransit;

public class DeliveryConsumer : IConsumer<ReserveDelivery>
{
    private readonly DeliveryDbContext _dbContext;

    public DeliveryConsumer(DeliveryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<ReserveDelivery> context)
    {
        // Симуляция резервирования курьера
        var courierAvailable = SimulateCourierAvailability(context.Message.TimeSlot);

        var delivery = new Delivery.Service.Models.Delivery
        {
            Id = Guid.NewGuid(),
            OrderId = context.Message.OrderId,
            TimeSlot = context.Message.TimeSlot,
            CourierId = courierAvailable ? "Courier1" : null,
            Status = courierAvailable ? DeliveryStatus.Reserved : DeliveryStatus.Cancelled
        };

        _dbContext.Deliveries.Add(delivery);
        await _dbContext.SaveChangesAsync();

        if (courierAvailable)
            await context.Publish(new DeliveryReserved (context.Message.OrderId ));
        else
            await context.Publish(new DeliveryReservationFailed( context.Message.OrderId,  "No free curiers for slot"));
    }

    private bool SimulateCourierAvailability(int timeSlot) => timeSlot % 2 == 0 ; // Успешно, если в четные слоты
}