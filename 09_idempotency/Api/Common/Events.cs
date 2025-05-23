namespace Common;

public record OrderCreated(Guid OrderId, Guid CustomerId, Guid ProductId, int Quantity, decimal Amount, int TimeSlot);
public record ProcessPayment(Guid OrderId, decimal Amount);
public record PaymentProcessed(Guid OrderId);
public record PaymentFailed(Guid OrderId, string Reason);
public record ReserveStock(Guid OrderId, Guid ProductId, int Quantity);
public record StockReserved(Guid OrderId);
public record StockReservationFailed(Guid OrderId, string Reason);
public record ReserveDelivery(Guid OrderId, int TimeSlot);
public record DeliveryReserved(Guid OrderId);
public record DeliveryReservationFailed(Guid OrderId, string Reason);
public record CancelPayment(Guid OrderId, string Reason);
public record ReleaseStock(Guid OrderId);
public record CancelOrder(Guid OrderId, string Reason);
public record CompleteOrder(Guid OrderId);