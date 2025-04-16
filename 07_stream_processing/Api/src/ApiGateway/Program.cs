var builder = WebApplication.CreateBuilder(args);

// Cofigures the service discovery services
builder.Services.AddServiceDiscovery();
// Настройка YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

// Подключение YARP
app.MapReverseProxy();
app.Run();