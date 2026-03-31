using System;
using System.Threading;
using System.Threading.Tasks;
using urban_dukan_checkout_service.Models;

namespace urban_dukan_checkout_service.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartAsync(int userId, CancellationToken ct = default);
        Task SaveCartAsync(Cart cart, CancellationToken ct = default);
        Task DeleteCartAsync(int userId, CancellationToken ct = default);
    }
}