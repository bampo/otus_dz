using MassTransit;

namespace Orders.Service.Saga;

public class OrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
    public int TimeSlot { get; set; }
    public DateTime? PaymentProcessed { get; set; }
    public DateTime? StockReserved { get; set; }
    public DateTime? DeliveryReserved { get; set; }
}