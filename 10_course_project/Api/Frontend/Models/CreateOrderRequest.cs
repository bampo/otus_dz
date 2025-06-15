using System.Collections.Generic;
using IShop.Frontend.Models;

namespace IShop.Frontend.Models
{
    public class CreateOrderRequest
    {
        public Guid CustomerId { get; set; }
        public int TimeSlot { get; set; }
        public Guid IdempotencyKey { get; set; } = Guid.NewGuid();
    }
}