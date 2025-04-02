using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("Default")
        .WithImage("postgres:16")
        //.WithPgAdmin()
        ;

var userprofileapi = builder.AddContainer("userprofileapi", "userprofileapi", "latest")
    .WithReference(postgres)
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "userapiep")
    .WaitFor(postgres);

var authapi = builder
    .AddContainer("authapi", "authapi", "latest")
    //.AddDockerfile("authapi", "../Auth", "../Auth/Auth.Api/Dockerfile")
    .WithReference(postgres)
    .WithHttpEndpoint(port: 8081, targetPort: 8080, name: "authep")
    .WaitFor(postgres);

// Добавляем Nginx как контейнер
var nginx = builder.AddContainer("apigateway", "nginx:latest")
    .WithBindMount("Config/nginx.conf", "/etc/nginx/nginx.conf")
    .WithHttpEndpoint(port: 80, targetPort: 80)
    .WaitFor(authapi)
    .WaitFor(userprofileapi)
; // Связываем с auth-service



await builder.Build().RunAsync();
