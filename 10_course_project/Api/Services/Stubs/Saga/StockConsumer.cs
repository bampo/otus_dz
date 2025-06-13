using Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Stubs.Service.DbContexts;
using Stubs.Service.Models;

namespace Stubs.Service.Saga;

public class StockConsumer(StubsDbContext dbContext, ILogger<StockConsumer> logger) : IConsumer<ReserveStocks>, IConsumer<ReleaseStock>
{

    public async Task Consume(ConsumeContext<ReserveStocks> context)
    {
        // Симуляция проверки наличия товара
        bool stockAvailable = AvailableStocksCheck(context.Message.OrderListId);

        var reservation = new StockReservation
        {
            Id = Guid.NewGuid(),
            OrderId = context.Message.OrderId,
            ProductId = context.Message.OrderListId,
            Status = stockAvailable ? "Reserved" : "Failed"
        };

        dbContext.StockReservations.Add(reservation);
        await dbContext.SaveChangesAsync();

        if (stockAvailable)
        {
            logger.LogInformation("Stocks reserved");
            await context.Publish(new Stockseserved (context.Message.OrderId ));
        }
        else
        {
            logger.LogWarning("Stocks reservation failed.");
            await context.Publish(new StockReservationFailed (context.Message.OrderId, "Stock reservation failed"));
        }
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

    private bool AvailableStocksCheck(Guid orderListId)
    {
        return true;
    }
}