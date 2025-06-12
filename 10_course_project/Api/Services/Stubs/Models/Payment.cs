namespace Stubs.Service.Models;

public class Payment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } // Pending, Completed, Failed
    public string? Reason { get; set; }
}