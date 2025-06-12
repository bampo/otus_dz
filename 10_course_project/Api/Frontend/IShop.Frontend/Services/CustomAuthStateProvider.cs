using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace IShop.Frontend.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly AuthService _authService;

    public CustomAuthStateProvider(IJSRuntime jsRuntime, AuthService authService)
    {
        _jsRuntime = jsRuntime;
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _authService.GetToken();
        
        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // In a real app, you would validate the token and extract claims
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "user@example.com"),
        }, "custom");

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}