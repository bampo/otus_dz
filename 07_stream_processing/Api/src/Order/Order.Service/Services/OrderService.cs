namespace Order.Service.Services;

public class OrdersService(ILogger<OrdersService> logger, IHttpClientFactory httpFactory)
{
    private readonly ILogger<OrdersService> _logger = logger;

    public async Task<object?> CreateOrder(string userId, decimal price)
    {
        var billing = httpFactory.CreateClient("billing-service");
        var req = new { userId, amount = -price };
        var depositResp = await billing.PostAsJsonAsync("/api/billing/deposit", req);
        
        if (depositResp.IsSuccessStatusCode)
        {
            var orderId = Guid.NewGuid();
            await PushNotification( userId, $"Order created: {orderId}, price: {price}");
            return orderId;
        }

        await PushNotification(userId, "Not enougth funds on deposit");
        return null;
    }

    private async Task PushNotification(string userId, string message)
    {
        var req = new { userId, message};
        var notify = httpFactory.CreateClient("notify-service");
        await notify.PostAsJsonAsync("/api/notify/notification", req);
    }
}