namespace Notification.Services;

public class EmailService(ILogger<EmailService> logger)
{
    public Task SendWelcomeEmail(string email, string name, Guid customerId)
    {
        var confirmLink = $"/api/customers/confirm-email/{customerId}";
        logger.LogInformation("Sending welcome email to {Email} ({Name}), confirm URI: '{urI}'", email, name, confirmLink);
        return Task.CompletedTask;
    }
}