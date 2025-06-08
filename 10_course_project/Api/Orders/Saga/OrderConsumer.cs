using Common;
using MassTransit;

namespace Orders.Service.Saga;

public class OrderConsumer(OrderDbContext dbContext) : IConsumer<CompleteOrder>, IConsumer<CancelOrder>
{

    public async Task Consume(ConsumeContext<CompleteOrder> context)
    {
        var order = await dbContext.Orders.FindAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = "Completed";
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task Consume(ConsumeContext<CancelOrder> context)
    {
        var order = await dbContext.Orders.FindAsync(context.Message.OrderId);
        if (order != null)
        {
            order.Status = "Failed";
            order.Reason = context.Message.Reason;
            await dbContext.SaveChangesAsync();
        }
    }
}