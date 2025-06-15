using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Microsoft.IdentityModel.JsonWebTokens;

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

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _authService.GetToken();
        
        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var jwtToken = new JsonWebToken(token);
            
            var claims = jwtToken.Claims.ToList();

            if (claims.All(c => c.Type != ClaimTypes.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, jwtToken.Subject ?? "anonymous"));
            }
            
            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            // If token is invalid, treat as anonymous
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}