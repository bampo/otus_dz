using Common;
using MassTransit;
using Notification.Services;

namespace Notification.Consumers;

public class CustomerRegisteredConsumer(EmailService emailService) : IConsumer<CustomerRegistered>
{
    public async Task Consume(ConsumeContext<CustomerRegistered> context)
    {
        await emailService.SendWelcomeEmail(
            context.Message.Email,
            $"{context.Message.FirstName} {context.Message.LastName}");
    }
}