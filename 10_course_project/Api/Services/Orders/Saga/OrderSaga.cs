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
    public Event<DeliveryReservationFailed> DeliveryReservationFailedEvent { get; private set; }

    public OrderSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderCreatedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentProcessedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => StocksReservedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => DeliveryReservedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentFailedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => StocksReservationFailedEvent, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => DeliveryReservationFailedEvent, x => x.CorrelateById(context => context.Message.OrderId));

        Initially(
            When(OrderCreatedEvent)
                .Then(context =>
                {
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.CustomerId = context.Message.CustomerId;
                    context.Saga.OrderListId = context.Message.OrderListId;
                    context.Saga.TimeSlot = context.Message.TimeSlot;
                })
                .Publish(context => new ProcessPayment( context.Saga.OrderId, context.Saga.Amount ))
                .TransitionTo(Pending));

        During(Pending,
            When(PaymentProcessedEvent)
                .Then(context => context.Saga.PaymentProcessed = DateTime.UtcNow)
                .Publish(context => new ReserveStocks (context.Saga.OrderId, context.Saga.OrderListId)),
            When(PaymentFailedEvent)
                .Publish(context => new CancelOrder (context.Saga.OrderId, context.Message.Reason ))
                .TransitionTo(Failed));

        During(Pending,
            When(StocksReservedEvent)
                .Then(context => context.Saga.StockReserved = DateTime.UtcNow)
                .Publish(context => new ReserveDelivery (context.Saga.OrderId, context.Saga.TimeSlot)),
            When(StocksReservationFailedEvent)
                .Publish(context => new CancelPayment (context.Saga.OrderId, context.Message.Reason))
                .Publish(context => new CancelOrder (context.Saga.OrderId, context.Message.Reason))
                .TransitionTo(Failed));

        During(Pending,
                When(DeliveryReservedEvent)
                    .Then(context => context.Saga.DeliveryReserved = DateTime.UtcNow)
                    .Publish(context => new CompleteOrder (context.Saga.OrderId))
                    .TransitionTo(Completed)
                    .Finalize(),
                When(DeliveryReservationFailedEvent)
                    .Publish(context => new CancelPayment (context.Saga.OrderId, context.Message.Reason))
                    .Publish(context => new ReleaseStock (context.Saga.OrderId))
                    .Publish(context => new CancelOrder (context.Saga.OrderId, context.Message.Reason))
                    .TransitionTo(Failed)
                    .Finalize())
            ;
    }
}