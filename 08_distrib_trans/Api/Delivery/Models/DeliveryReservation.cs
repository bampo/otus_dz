namespace Delivery.Service.Models;

public class DeliveryReservation
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public int TimeSlot { get; set; }
}