using System;
using System.Threading;
using System.Threading.Tasks;
using urban_dukan_checkout_service.Models;

namespace urban_dukan_checkout_service.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order, CancellationToken ct = default);
        Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Order[]> GetByUserAsync(int userId, CancellationToken ct = default);
    }
}