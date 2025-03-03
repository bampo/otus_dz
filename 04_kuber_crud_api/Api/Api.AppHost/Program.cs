using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("Default")
        .WithImage("postgres:16")
        //.WithPgAdmin()
        ;

builder.AddProject<Projects.CrudApi>("crudapi")
    .WithReference(postgres);

await builder.Build().RunAsync();
