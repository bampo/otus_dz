namespace Orders.Service.Models;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; init; }
    public string ProductName { get; set; }
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}