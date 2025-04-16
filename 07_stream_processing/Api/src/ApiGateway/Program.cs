var builder = WebApplication.CreateBuilder(args);

// Cofigures the service discovery services
builder.Services.AddServiceDiscovery();
// ��������� YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

// ����������� YARP
app.MapReverseProxy();
app.Run();