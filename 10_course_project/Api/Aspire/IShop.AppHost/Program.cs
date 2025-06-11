var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("username", secret: false);
var password = builder.AddParameter("password", secret: false);
var rabbitMq = builder.AddRabbitMQ("rabbitmq",username, password)
    .WithManagementPlugin(5100)
    ;

var pgPass = builder.AddParameter("pgPass", "postgres");
var pgUser = builder.AddParameter("pgUser", "postgres");
var postgres = builder.AddPostgres("pgsrv",pgUser,pgPass, 5432)
        .WithImage("postgres:16")
        // .WithEnvironment("POSTGRES_USERNAME", username)
        // .WithEnvironment("POSTGRES_PASSWORD", password)
        
        //.WithEndpoint(5432, name:"pg-tcp", targetPort: 5432)
        //.WithPgWeb(cfg => cfg.WithHostPort(5050))
    ;
var orderdb = postgres.AddDatabase("ordersdb");
var stubsdb = postgres.AddDatabase("stubsdb");
var cartdb = postgres.AddDatabase("cartdb");
var catalogdb = postgres.AddDatabase("catalogdb");
var customerdb = postgres.AddDatabase("customerdb");

var orders = builder.AddProject<Projects.Orders_Service>("orders-service")
    .WithReference(rabbitMq)
    .WithReference(orderdb)
    .WaitFor(orderdb);

var notification = builder.AddProject<Projects.Notification_Service>("notification-service")
    .WithReference(rabbitMq);


var stubs = builder.AddProject<Projects.Stubs_Service>("stubs-service")
    .WithReference(rabbitMq)
    .WithReference(stubsdb)
    .WaitFor(stubsdb);

var cart = builder.AddProject<Projects.Cart_Service>("cart-service")
    .WithReference(rabbitMq)
    .WithReference(cartdb)
    .WaitFor(cartdb);

var catalog = builder.AddProject<Projects.Catalog_Service>("catalog-service")
    .WithReference(rabbitMq)
    .WithReference(catalogdb)
    .WaitFor(catalogdb);

var customer = builder.AddProject<Projects.Customer_Service>("customer-service")
    .WithReference(rabbitMq)
    .WithReference(customerdb)
    .WaitFor(customerdb);

builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithReference(orders)
    .WithReference(stubs)
    .WithReference(cart)
    .WithReference(catalog)
    .WithReference(customer)
    .WithReference(notification)
    ;

builder.Build().Run();
