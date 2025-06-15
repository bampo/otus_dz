using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly Random random = new Random();

    static async Task Main(string[] args)
    {
        while (true)
        {
            try
            {
                var requestData = new { email = "a@a.com", password = "12345" };
                if (random.Next(100) > 80)
                {
                    requestData = new { email = "a@a.com", password = "bad" };
                }
                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("http://arch.homework/api/customers/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"{DateTime.Now} - Request successful");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now} - Request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} - Error: {ex.Message}");
            }

            // Random delay between 200ms and 1000ms
            int delay = random.Next(200, 1001);
            await Task.Delay(delay);
        }
    }
}
