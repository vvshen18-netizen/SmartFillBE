using Microsoft.EntityFrameworkCore;
using AuthAPI.Models;
using SmarfillAPI.Models;

namespace AuthAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<DeliveryGuy> DeliveryGuys { get; set; }

        public DbSet<FuelPrice> FuelPrices { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<OrderRejection> OrderRejections { get; set; }

        public DbSet<Delivery> Deliveries { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }

    }
}
