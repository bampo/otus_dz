using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IShop.Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace IShop.Frontend.Services
{
    public class OrderService
    {
        private readonly HttpClient _http;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IJSRuntime _jsRuntime;

        public OrderService(HttpClient http, AuthenticationStateProvider authStateProvider, IJSRuntime jsRuntime)
        {
            _http = http;
            _authStateProvider = authStateProvider;
            _jsRuntime = jsRuntime;
        }

        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest orderRequest)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User must be logged in to create order");
            }


            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            _http.DefaultRequestHeaders.Add("Idempotency-Key", orderRequest.IdempotencyKey.ToString());

            orderRequest.CustomerId = Guid.Parse(userId);

            var response = await _http.PostAsJsonAsync("api/orders", orderRequest);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OrderResponse>();
        }
    
        public class OrderResponse
        {
            public Guid Id { get; set; }
            public Guid CustomerId { get; set; }
            public decimal Amount { set; get; }
            public virtual ICollection<OrderItem> OrderItems { get; set; }
        }
    
        public class OrderItem
        {
            public Guid Id { get; set; }
            public Guid ProductId { get; init; }
            public int Quantity { get; init; }
            public decimal Price { get; init; }
        }
    
        public async Task<List<OrderWithItems>> GetOrdersAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User.FindFirst("user_id")?.Value;
            
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User must be logged in");
                
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            return await _http.GetFromJsonAsync<List<OrderWithItems>>($"api/orders/{userId}");
        }
    }


}
