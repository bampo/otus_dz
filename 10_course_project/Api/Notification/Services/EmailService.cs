namespace Notification.Services;

public class EmailService(ILogger<EmailService> logger)
{
    public Task SendWelcomeEmail(string email, string name)
    {
        logger.LogInformation("Sending welcome email to {Email} ({Name})", email, name);
        return Task.CompletedTask;
    }
}