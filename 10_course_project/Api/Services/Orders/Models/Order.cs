#nullable disable
namespace Orders.Service.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid OrderListId { get; set; }
    public string Status { get; set; } // Pending, Completed, Failed
    public string Reason { get; set; }
    public int TimeSlot { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string IdempotencyKey { get; set; }
}