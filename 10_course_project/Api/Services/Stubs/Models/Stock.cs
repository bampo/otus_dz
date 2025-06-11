namespace Stubs.Service.Models;

public class Stock
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}