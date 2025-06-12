namespace Orders.Service.Models;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}