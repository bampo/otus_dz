using System.Net;
using System.Net.Http.Json;
using IShop.Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace IShop.Frontend.Services;

public class CartService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IJSRuntime _jsRuntime;
    private const string AuthTokenKey = "authToken";

    public event EventHandler CartUpdated;

    protected virtual void OnCartUpdated()
    {
        CartUpdated?.Invoke(this, EventArgs.Empty);
    }

    public CartService(HttpClient httpClient,
        AuthenticationStateProvider authStateProvider, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
        _jsRuntime = jsRuntime;
    }

    public async Task AddToCartAsync(Guid productId, int quantity = 1)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirst("user_id")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User must be logged in to add items to cart");
        }

        var request = new AddToCartRequest
        {
            ProductId = productId,
            CustomerId = Guid.Parse(userId),
            Quantity = quantity
        };

        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", AuthTokenKey);
        _httpClient.DefaultRequestHeaders.Authorization = (new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token));
        var response = await _httpClient.PostAsJsonAsync("api/cart", request);
        response.EnsureSuccessStatusCode();
        OnCartUpdated();
    }

    public async Task UpdateCartItemAsync(CartItem item)
    {
        await AddToCartAsync(item.ProductId, item.Quantity);
    }

    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirst("user_id")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User must be logged in to view cart");
        }

        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", AuthTokenKey);
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/cart/{userId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<CartItem>>() ?? new List<CartItem>();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<CartItem>();
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to fetch cart items: " + ex.Message);
        }
    }

    public async Task RemoveCartItem(CartItem item)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var userId = authState.User.FindFirst("user_id")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User must be logged in to update cart");
        }

        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", AuthTokenKey);
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var response = await _httpClient.DeleteAsync($"api/cart/{userId}/{item.Id}");
        response.EnsureSuccessStatusCode();
        OnCartUpdated();
    }
}