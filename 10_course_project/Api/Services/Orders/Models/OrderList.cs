namespace Orders.Service.Models;

public class OrderList
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}