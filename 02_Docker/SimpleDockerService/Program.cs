using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.ConfigureKestrel(
    opts =>
    {
        opts.ListenAnyIP(8000);
    });
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

app.MapGet("/health", () => new State("OK"));
app.MapGet("/", () => """
                      Hello from Simple Docker Web Service.
                      /helth - check health
                      /todos - show TODOs
                      /todos/{id} - show TODO with id
                      """);

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => Data.SampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    Data.SampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(State))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{ }



public record State(string Status);

internal static class Data
{
    public static Todo[] SampleTodos = new Todo[]
    {
        new(1, "Walk the dog"),
        new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
        new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
        new(4, "Clean the bathroom"),
        new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
    };
}