#nullable disable
namespace Stubs.Service.Models;

public class StockReservation
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } // Reserved, Released
}