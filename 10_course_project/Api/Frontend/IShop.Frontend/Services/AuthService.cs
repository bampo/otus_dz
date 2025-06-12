using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using Microsoft.JSInterop;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace IShop.Frontend.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private const string AuthTokenKey = "authToken";

    public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> RegisterUser(RegisterModel model)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/customers/auth/register", model);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<JsonNode>();
                return json?["customerId"]?.ToString();
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Registration failed: " + ex.Message);
        }
    }

    public async Task<string> Login(string email, string password)
    {
        try
        {
            var loginRequest = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/customers/auth/login", loginRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                await StoreToken(loginResponse.Token);
                return loginResponse.Token;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorMessage);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Login failed: " + ex.Message);
        }
    }

    public async Task Logout()
    {
        await RemoveToken();
    }

    private async Task StoreToken(string token)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AuthTokenKey, token);
    }

    private async Task RemoveToken()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AuthTokenKey);
    }

    public async Task<string> GetToken()
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", AuthTokenKey);
    }
}

public record LoginResponse(string Token);
public record RegisterModel(
    string FirstName,
    string LastName,
    string Email,
    string Password);