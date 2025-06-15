using Common;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Stubs.Service.DbContexts;
using Stubs.Service.Models;

namespace Stubs.Service.Saga;

public class PaymentConsumer(StubsDbContext dbContext, ILogger<PaymentConsumer> logger) : IConsumer<ProcessPayment>, IConsumer<CancelPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        // Симуляция проверки платежа
        var paymentMessage = SimulatePaymentProcessing(context.Message.Amount);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = context.Message.OrderId,
            Amount = context.Message.Amount,
            Status = paymentMessage is null ? "Completed" : "Failed"
        };

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        if (paymentMessage == null)
        {
            logger.LogInformation("Payment was successful");
            await context.Publish(new PaymentProcessed( context.Message.OrderId ));
        }
        else
        {
            logger.LogWarning("Payment failed: {msg}", paymentMessage);
            await context.Publish(new PaymentFailed (context.Message.OrderId, $"Платеж отклонен: {paymentMessage}"));
        }
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

    // Проходит, если стоимость меньше 301
    private string? SimulatePaymentProcessing(decimal Amount) => Amount < 300 
        ? null    
        : "Недостаточно средств (< 300)"
    ; 
}