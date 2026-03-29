using Microsoft.EntityFrameworkCore;
using urban_dukan_checkout_service.Models;

namespace urban_dukan_checkout_service.Data
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(b =>
            {
                b.HasKey(o => o.Id);
                b.HasMany(o => o.Items)
                 .WithOne(i => i.Order)
                 .HasForeignKey(i => i.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(b =>
            {
                b.HasKey(i => i.Id);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}