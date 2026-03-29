using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using urban_dukan_checkout_service.Configurations;
using urban_dukan_checkout_service.Models;

namespace urban_dukan_checkout_service.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IDatabase _db;
        private readonly ILogger<CartRepository> _logger;
        private readonly RedisSettings _settings;

        public CartRepository(IConnectionMultiplexer multiplexer, IOptions<RedisSettings> settings, ILogger<CartRepository> logger)
        {
            _db = multiplexer.GetDatabase();
            _settings = settings.Value;
            _logger = logger;
        }

        private static string GetKey(Guid userId) => $"cart:{userId}";

        public async Task<Cart?> GetCartAsync(Guid userId, CancellationToken ct = default)
        {
            var key = GetKey(userId);
            var raw = await _db.StringGetAsync(key);
            if (!raw.HasValue) return null;
            try
            {
                return JsonSerializer.Deserialize<Cart>(raw!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize cart for {UserId}", userId);
                return null;
            }
        }

        public async Task SaveCartAsync(Cart cart, CancellationToken ct = default)
        {
            var key = GetKey(cart.UserId);
            var json = JsonSerializer.Serialize(cart);
            var expiry = TimeSpan.FromHours(_settings.CartTtlHours);
            await _db.StringSetAsync(key, json, expiry);
        }

        public async Task DeleteCartAsync(Guid userId, CancellationToken ct = default)
        {
            var key = GetKey(userId);
            await _db.KeyDeleteAsync(key);
        }
    }
}