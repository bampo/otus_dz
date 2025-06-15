using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using IShop.Frontend;
using IShop.Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped(sp =>
{
    var client = new HttpClient
    {
        BaseAddress = new Uri(builder.Configuration["ApiGatewayUrl"])
    };
    return client;
});


// Register AuthService
builder.Services.AddScoped<AuthService>();

// Register CatalogService with IJSRuntime
builder.Services.AddScoped<CatalogService>(sp =>
    new CatalogService(sp.GetRequiredService<HttpClient>(),
                      sp.GetRequiredService<IJSRuntime>()));


builder.Services
    .AddScoped<CartService>()
    .AddScoped<OrderService>();

// Add authentication services
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

var app = builder.Build();

await app.RunAsync();
