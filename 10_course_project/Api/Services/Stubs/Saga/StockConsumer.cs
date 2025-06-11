using Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Stubs.Service.DbContexts;
using Stubs.Service.Models;

namespace Stubs.Service.Saga;

public class StockConsumer(StubsDbContext dbContext) : IConsumer<ReserveStock>, IConsumer<ReleaseStock>
{

    public async Task Consume(ConsumeContext<ReserveStock> context)
    {
        // Симуляция проверки наличия товара
        bool stockAvailable = SimulateStockCheck(context.Message.Quantity);

        var reservation = new StockReservation
        {
            Id = Guid.NewGuid(),
            OrderId = context.Message.OrderId,
            ProductId = context.Message.ProductId,
            Quantity = context.Message.Quantity,
            Status = stockAvailable ? "Reserved" : "Failed"
        };

        dbContext.StockReservations.Add(reservation);
        await dbContext.SaveChangesAsync();

        if (stockAvailable)
            await context.Publish(new StockReserved (context.Message.OrderId ));
        else
            await context.Publish(new StockReservationFailed (context.Message.OrderId, "Stock reservation failed"));
    }

    public async Task Consume(ConsumeContext<ReleaseStock> context)
    {
        var reservation = await dbContext.StockReservations.FirstOrDefaultAsync(r => r.OrderId == context.Message.OrderId);
        if (reservation != null)
        {
            reservation.Status = "Released";
            await dbContext.SaveChangesAsync();
        }
    }

    private bool SimulateStockCheck(int quantity) => quantity <= 10; // Симуляция наличия
}