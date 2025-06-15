```
mermaid

graph TD
    subgraph Сервисы
        ApiGateway -->|Запросы| CartService
        ApiGateway -->|Запросы| CatalogService
        ApiGateway -->|Запросы| CustomerService
        ApiGateway -->|Запросы| NotificationService
        ApiGateway -->|Запросы| OrdersService
        ApiGateway -->|Запросы| StubsService

        CartService -->|События| NotificationService
        CatalogService -->|События| NotificationService
        CustomerService -->|События| NotificationService
        OrdersService -->|События| NotificationService
        StubsService -->|События| NotificationService

        CartService -->|Доступ к данным| CartDb
        CatalogService -->|Доступ к данным| CatalogDb
        CustomerService -->|Доступ к данным| CustomerDb
        OrdersService -->|Доступ к данным| OrderDb
        StubsService -->|Доступ к данным| StubsDb

        subgraph OrderSaga
            StubsService -->|Stock| StockComponent
            StubsService -->|Payment| PaymentComponent
            StubsService -->|Delivery| DeliveryComponent
        end
    end


```