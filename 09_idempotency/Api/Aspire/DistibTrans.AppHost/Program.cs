using Aspire.Hosting;

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
var paymentsdb = postgres.AddDatabase("paymentsdb");
var deliverydb = postgres.AddDatabase("deliverydb");
var stocksdb = postgres.AddDatabase("stocksdb");

var orders = builder.AddProject<Projects.Orders_Service>("orders-service")
    .WithReference(rabbitMq)
    .WithReference(orderdb)
    .WaitFor(orderdb);

var payments = builder.AddProject<Projects.Payments_Service>("payments-service")
    .WithReference(rabbitMq)
    .WithReference(paymentsdb)
    .WaitFor(paymentsdb);

var stocks = builder.AddProject<Projects.Stocks_Service>("stocks-service")
    .WithReference(rabbitMq)
    .WithReference(stocksdb)
    .WaitFor(stocksdb);

var delivary = builder.AddProject<Projects.Delivery_Service>("delivery-service")
    .WithReference(rabbitMq)
    .WithReference(deliverydb)
    .WaitFor(deliverydb);

builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithReference(orders)
    .WithReference(payments)
    .WithReference(delivary)
    .WithReference(stocks)
    ;

builder.Build().Run();
