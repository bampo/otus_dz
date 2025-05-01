using Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Stocks;
using Stocks.Models;

public class StockConsumer : IConsumer<ReserveStock>, IConsumer<ReleaseStock>
{
    private readonly WarehouseDbContext _dbContext;

    public StockConsumer(WarehouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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

        _dbContext.StockReservations.Add(reservation);
        await _dbContext.SaveChangesAsync();

        if (stockAvailable)
            await context.Publish(new StockReserved (context.Message.OrderId ));
        else
            await context.Publish(new StockReservationFailed (context.Message.OrderId, "Stock reservation failed"));
    }

    public async Task Consume(ConsumeContext<ReleaseStock> context)
    {
        var reservation = await _dbContext.StockReservations.FirstOrDefaultAsync(r => r.OrderId == context.Message.OrderId);
        if (reservation != null)
        {
            reservation.Status = "Released";
            await _dbContext.SaveChangesAsync();
        }
    }

    private bool SimulateStockCheck(int quantity) => quantity <= 10; // Симуляция наличия
}