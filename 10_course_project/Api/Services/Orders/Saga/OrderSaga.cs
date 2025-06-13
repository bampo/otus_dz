#nullable disable
using Common;
using MassTransit;

namespace Orders.Service.Saga;

public class OrderSaga : MassTransitStateMachine<OrderSagaState>
{
    public State Pending { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }

    public Event<OrderCreated> OrderCreatedEvent { get; private set; }
    public Event<PaymentProcessed> PaymentProcessedEvent { get; private set; }
    public Event<Stockseserved> StocksReservedEvent { get; private set; }
    public Event<DeliveryReserved> DeliveryReservedEvent { get; private set; }
    public Event<PaymentFailed> PaymentFailedEvent { get; private set; }
    public Event<StockReservationFailed> StocksReservationFailedEvent { get; private set; }
    public Event<DeliveryCancelled> DeliveryCancelledEvent { get; private set; }

    public OrderSaga(ILogger<OrderSaga> logger)
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderCreatedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentProcessedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => StocksReservedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => DeliveryReservedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentFailedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => StocksReservationFailedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => DeliveryCancelledEvent, x => x.CorrelateById(context => context.Message.OrderId));

        Initially(
            When(OrderCreatedEvent)
                .Then(context =>
                {
                    logger.LogInformation("OrderCeatedEvent received.");
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CustomerId = context.Message.CustomerId;
                    context.Saga.OrderListId = context.Message.OrderListId;
                    context.Saga.TimeSlot = context.Message.TimeSlot;
                    context.Saga.Amount = context.Message.Amount;
                })
                .Publish(context => new ProcessPayment( context.Saga.OrderId, context.Saga.Amount ))
                .TransitionTo(Pending));

        During(Pending,
            When(PaymentProcessedEvent)
                .Then(context =>
                {
                    logger.LogInformation("PaymentProcessedEvent received.");
                    context.Saga.PaymentProcessed = DateTime.UtcNow;
                })
                .Publish(context => new ReserveStocks (context.Saga.OrderId, context.Saga.OrderListId)),
            When(PaymentFailedEvent)
                .Publish(context =>
                {
                    logger.LogWarning("PaymentFailedEvent received. Rollback order.");
                    return new CancelOrder(context.Saga.OrderId, context.Message.Reason);
                })
                .TransitionTo(Failed));

        During(Pending,
            When(StocksReservedEvent)
                .Then(context =>
                {
                    logger.LogInformation("StocksReservedEvent received.");
                    context.Saga.StockReserved = DateTime.UtcNow;
                })
                .Publish(context => new ReserveDelivery (context.Saga.OrderId, context.Saga.TimeSlot)),
            When(StocksReservationFailedEvent)
                .Then(
                    context =>
                    {
                        logger.LogWarning("StocksReservationFailedEvent received.  Rollback order.");
                    })
                .Publish(context => new CancelPayment (context.Saga.OrderId, context.Message.Reason))
                .Publish(context => new CancelOrder (context.Saga.OrderId, context.Message.Reason))
                .TransitionTo(Failed));

        During(Pending,
                When(DeliveryReservedEvent)
                    .Then(context =>
                    {
                        logger.LogInformation("DeliveryReservedEvent received");
                        context.Saga.DeliveryReserved = DateTime.UtcNow;
                    })
                    .Publish(context => new CompleteOrder (context.Saga.OrderId))
                    .TransitionTo(Completed)
                    .Finalize(),
                When(DeliveryCancelledEvent)
                    .Then(
                        context =>
                        {
                            logger.LogWarning("DeliveryCancelledEvent received. Rollback order.");
                        })
                    .Publish(context => new CancelPayment (context.Saga.OrderId, context.Message.Reason))
                    .Publish(context => new ReleaseStock (context.Saga.OrderId))
                    .Publish(context => new CancelOrder (context.Saga.OrderId, context.Message.Reason))
                    .TransitionTo(Failed)
                    .Finalize())
            ;
    }
}