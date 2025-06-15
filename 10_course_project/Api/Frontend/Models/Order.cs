namespace IShop.Frontend.Models
{
    public class OrderWithItems
    {
        public Order Order { get; set; }
        public IList<OrderItem> OrderItems { get; set; }
    }
    public class Order
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public int TimeSlot { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; }
    }
}