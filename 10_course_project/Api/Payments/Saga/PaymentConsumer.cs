using Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Payments.Service.Saga;

public class PaymentConsumer(PaymentDbContext dbContext) : IConsumer<ProcessPayment>, IConsumer<CancelPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        // Симуляция проверки платежа
        var paymentSuccess = SimulatePaymentProcessing(context.Message.Amount);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = context.Message.OrderId,
            Amount = context.Message.Amount,
            Status = paymentSuccess ? "Completed" : "Failed"
        };

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        if (paymentSuccess)
            await context.Publish(new PaymentProcessed( context.Message.OrderId ));
        else
            await context.Publish(new PaymentFailed (context.Message.OrderId, "Payment failed"));
    }

    public async Task Consume(ConsumeContext<CancelPayment> context)
    {
        var payment = await dbContext.Payments.FirstOrDefaultAsync(p => p.OrderId == context.Message.OrderId);
        if (payment != null)
        {
            payment.Status = "Failed";
            payment.Reason = context.Message.Reason;
            await dbContext.SaveChangesAsync();
        }
    }

    private bool SimulatePaymentProcessing(decimal Amount) => Amount < 100; // Проходит, если стоимость меньше 100
}