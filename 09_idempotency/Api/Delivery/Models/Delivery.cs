namespace Delivery.Service.Models;

public class Delivery
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public int TimeSlot { get; set; }
    public string? CourierId { get; set; }
    public DeliveryStatus Status { get; set; } // Reserved, Canceled
}

public enum DeliveryStatus
{
    Reserved = 0,
    Cancelled = 1
}
