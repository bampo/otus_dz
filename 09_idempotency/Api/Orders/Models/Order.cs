namespace Orders.Service.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } // Pending, Completed, Failed
    public string? Reason { get; set; }
    public int TimeSlot { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string IdempotencyKey { get; set; }
}