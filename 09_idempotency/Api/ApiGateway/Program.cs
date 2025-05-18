var builder = WebApplication.CreateBuilder(args);

// Cofigures the service discovery services
builder.Services.AddServiceDiscovery();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.MapReverseProxy();
app.Run();