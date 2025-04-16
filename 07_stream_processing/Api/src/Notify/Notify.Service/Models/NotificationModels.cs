using System;

namespace Notify.Service.Models
{
    public class NotificationRequest
    {
        public string userId { get; set; }
        public string message { get; set; }
    }

    public class NotificationResponse
    {
        public int Id { get; set; }
        public string message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
