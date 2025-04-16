using System.Text.Json;

namespace User.Service;

public class BillingService(HttpClient httpClient)
{
    public async Task<object> CreateUser(string userId)
    {
        var user = new { userId = userId };

        var response = await httpClient.PostAsJsonAsync("/api/billing/create-user", user);
        response.EnsureSuccessStatusCode();
        return response.Content.ReadFromJsonAsync<object>();
    }
}

