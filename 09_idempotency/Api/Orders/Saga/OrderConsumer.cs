using Common;
using MassTransit;

namespace Orders.Service.Saga;

public class OrderConsumer : IConsumer<CompleteOrder>, IConsumer<CancelOrder>
{
    private readonly OrderDbContext _dbContext;

    public OrderConsumer(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<CompleteOrder> context)
    {
        var order = await _dbContext.Orders.FindAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = "Completed";
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task Consume(ConsumeContext<CancelOrder> context)
    {
        var order = await _dbContext.Orders.FindAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = "Failed";
            order.Reason = context.Message.Reason;
            await _dbContext.SaveChangesAsync();
        }
    }
}