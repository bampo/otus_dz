using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("Default")
        .WithImage("postgres:16")
        //.WithPgAdmin()
        ;

builder.AddProject<Projects.UserProfile_Api>("userprofileapi")
    .WithReference(postgres);

await builder.Build().RunAsync();
