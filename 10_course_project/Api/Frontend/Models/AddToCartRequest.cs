namespace IShop.Frontend.Models;

public class AddToCartRequest
{
    public Guid ProductId { get; set; }
    public Guid CustomerId { get; set; }
    public int Quantity { get; set; } = 1;
}