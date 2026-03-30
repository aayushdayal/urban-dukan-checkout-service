using Microsoft.EntityFrameworkCore;
using urban_dukan_checkout_service.Models;

namespace urban_dukan_checkout_service.Data
{
    public class OrdersDbContext : DbContext
    {
        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    }
}