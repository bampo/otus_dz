namespace Delivery.Service.Models;

public class DeliverySlot
{
    public Guid Id { get; set; }
    public int TimeSlot { get; set; }
    public bool IsAvailable { get; set; }
}