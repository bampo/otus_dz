using Microsoft.EntityFrameworkCore;
using Users.Dal.Entities;

namespace Users.Dal
{
    public class UsersDbContext(string connectionString) : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ProfileInfo> UserProfiles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString);

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Устанавливаем значение по умолчанию
        }
    }
}
