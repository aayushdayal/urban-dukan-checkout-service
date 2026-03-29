using System;
using System.Threading;
using System.Threading.Tasks;
using urban_dukan_checkout_service.DTOs;

namespace urban_dukan_checkout_service.Services
{
    public interface IOrderService
    {
        Task<CreateOrderResponse> CreateOrderAsync(Guid userId, CancellationToken ct = default);
        Task<OrderResponse?> GetOrderByIdAsync(Guid id, CancellationToken ct = default);
        Task<OrderResponse[]> GetOrdersByUserAsync(Guid userId, CancellationToken ct = default);
    }
}