namespace Orders.Service.Models;

public class OrderList
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal Amount { set; get; }
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}