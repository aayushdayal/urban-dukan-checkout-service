using System;
using System.Threading;
using System.Threading.Tasks;
using urban_dukan_checkout_service.DTOs;
using urban_dukan_checkout_service.Models;

namespace urban_dukan_checkout_service.Services
{
    public interface ICartService
    {
        Task<CartResponse?> GetCartAsync(int userId, CancellationToken ct = default);
        Task AddItemAsync(AddCartItemRequest request, CancellationToken ct = default);
        Task UpdateItemAsync(UpdateCartItemRequest request, CancellationToken ct = default);
        Task RemoveItemAsync(int userId, int productId, CancellationToken ct = default);
        Task ClearCartAsync(int userId, CancellationToken ct = default);
    }
}