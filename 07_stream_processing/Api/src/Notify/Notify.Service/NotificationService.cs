using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notify.Dal;

namespace Notify.Service
{
    public class NotificationService
    {
        private readonly NotifyDbContext _context;

        public NotificationService(NotifyDbContext context)
        {
            _context = context;
        }

        public async Task AddNotificationAsync(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetNotificationsAsync()
        {
            return await _context.Notifications.ToListAsync();
        }
    }
}
