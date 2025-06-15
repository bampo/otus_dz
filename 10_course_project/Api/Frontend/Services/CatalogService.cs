using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using Microsoft.JSInterop;

namespace IShop.Frontend.Services;

public class CatalogService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private const string AuthTokenKey = "authToken";

    public CatalogService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<List<CatalogItem>> GetCatalogItemsAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", AuthTokenKey);
            var request = new HttpRequestMessage(HttpMethod.Get, "api/catalog/list");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<CatalogItem>>() ?? new List<CatalogItem>();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<CatalogItem>();
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to fetch catalog items: " + ex.Message);
        }
    }
}

public class CatalogItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public int BuyQuantity { get; set; } = 1;
    public string ImageUrl { get; set; }
}