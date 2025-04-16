using System;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Notify.Dal
{
    public class NotifyDbContext : DbContext
    {
        public DbSet<Notification> Notifications { get; set; }

        public NotifyDbContext(DbContextOptions<NotifyDbContext> options) : base(options)
        {
        }
    }

    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
    }
}

