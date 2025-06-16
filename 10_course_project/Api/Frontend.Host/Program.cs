var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    //app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

//app.UseRouting();

app.MapFallbackToFile("index.html");

app.Run();
