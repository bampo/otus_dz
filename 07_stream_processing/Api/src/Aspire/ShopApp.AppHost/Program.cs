using Aspire.Hosting;
var builder = DistributedApplication.CreateBuilder(args);

var billingService = builder.AddProject<Projects.Billing_Service>("billing-service");

var notifyService = builder.AddProject<Projects.Notify_Service>("notify-service");

var userService = builder.AddProject<Projects.User_Service>("user-service")
        .WithReference(billingService)
    ;

var orderServcie = builder.AddProject<Projects.Order_Service>("order-service")
    .WithReference(billingService)
    .WithReference(notifyService)
    ;    ;


builder.AddProject<Projects.ApiGateway>("apigateway")
    .WithReference(billingService)
    .WithReference(orderServcie)
    .WithReference(notifyService)
    .WithReference(userService)
    ;


builder.Build().Run();
