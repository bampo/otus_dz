using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Billing.Dal;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();
builder.Configuration.AddUserSecrets<Program>();


InitDb(builder);

builder.Services.AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapControllers();

app.Run();

void InitDb(WebApplicationBuilder builder){
    
    var password = builder.Configuration["DB_PASSWORD"];
    var constring = builder.Configuration.GetConnectionString("Default").Replace("{DB_PASSWORD}", password);
    builder.Services.AddDbContext<BillngDbContext>(options => options.UseNpgsql(constring));

    using var scope = builder.Services.BuildServiceProvider().CreateScope();
    var db = scope.ServiceProvider.GetService<BillngDbContext>();
    //db.Database.EnsureDeleted(); 
    db.Database.EnsureCreated();
}
