using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using urban_dukan_checkout_service.DTOs;
using urban_dukan_checkout_service.Models;
using urban_dukan_checkout_service.Repositories;

namespace urban_dukan_checkout_service.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<CartResponse?> GetCartAsync(Guid userId, CancellationToken ct = default)
        {
            var cart = await _cartRepository.GetCartAsync(userId, ct);
            if (cart == null) return new CartResponse { UserId = userId };
            return new CartResponse
            {
                UserId = cart.UserId,
                Items = cart.Items.Select(i => new DTOs.CartItemDto { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
            };
        }

        public async Task AddItemAsync(AddCartItemRequest request, CancellationToken ct = default)
        {
            var cart = await _cartRepository.GetCartAsync(request.UserId, ct) ?? new Cart { UserId = request.UserId };

            var existing = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (existing != null)
            {
                existing.Quantity += request.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity });
            }

            await _cartRepository.SaveCartAsync(cart, ct);
        }

        public async Task UpdateItemAsync(UpdateCartItemRequest request, CancellationToken ct = default)
        {
            var cart = await _cartRepository.GetCartAsync(request.UserId, ct);
            if (cart == null) throw new InvalidOperationException("Cart not found");

            var existing = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (existing == null) throw new InvalidOperationException("Item not in cart");

            existing.Quantity = request.Quantity;
            await _cartRepository.SaveCartAsync(cart, ct);
        }

        public async Task RemoveItemAsync(Guid userId, int productId, CancellationToken ct = default)
        {
            var cart = await _cartRepository.GetCartAsync(userId, ct);
            if (cart == null) return;
            cart.Items.RemoveAll(i => i.ProductId == productId);
            await _cartRepository.SaveCartAsync(cart, ct);
        }

        public async Task ClearCartAsync(Guid userId, CancellationToken ct = default)
        {
            await _cartRepository.DeleteCartAsync(userId, ct);
        }
    }
}