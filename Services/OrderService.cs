using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using urban_dukan_checkout_service.Clients;
using urban_dukan_checkout_service.DTOs;
using urban_dukan_checkout_service.Models;
using urban_dukan_checkout_service.Repositories;
using urban_dukan_checkout_service.Configurations;

namespace urban_dukan_checkout_service.Services
{
    public class OrderService : IOrderService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IProductServiceClient _productClient;
        private readonly IOrderRepository _orderRepo;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            ICartRepository cartRepo,
            IProductServiceClient productClient,
            IOrderRepository orderRepo,
            ILogger<OrderService> logger)
        {
            _cartRepo = cartRepo;
            _productClient = productClient;
            _orderRepo = orderRepo;
            _logger = logger;
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(Guid userId, CancellationToken ct = default)
        {
            if (userId == Guid.Empty) throw new ArgumentException("userId required");

            var cart = await _cartRepo.GetCartAsync(userId, ct);
            if (cart == null || !cart.Items.Any()) throw new InvalidOperationException("Cart is empty");

            // Fetch product data
            var productIds = cart.Items.Select(i => i.ProductId).ToArray();
            var products = await _productClient.GetProductsAsync(productIds, ct);

            // Validate and prepare order items
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var cartItem in cart.Items)
            {
                if (!products.TryGetValue(cartItem.ProductId, out var prod))
                {
                    throw new InvalidOperationException($"Product {cartItem.ProductId} not found");
                }

                if (prod.Stock < cartItem.Quantity)
                {
                    throw new InvalidOperationException($"Product {prod.ProductId} stock insufficient");
                }

                var oi = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = prod.ProductId,
                    ProductName = prod.Name,
                    PriceAtPurchase = prod.Price,
                    Quantity = cartItem.Quantity
                };
                order.Items.Add(oi);
            }

            order.Total = order.Items.Sum(i => i.PriceAtPurchase * i.Quantity);

            // Persist order
            var created = await _orderRepo.CreateOrderAsync(order, ct);

            // Clear cart
            await _cartRepo.DeleteCartAsync(userId, ct);

            return new CreateOrderResponse { OrderId = created.Id };
        }

        // New: Create order directly for a single product without persisting to cart
        public async Task<CreateOrderResponse> CreateOrderForSingleItemAsync(Guid userId, int productId, int quantity, CancellationToken ct = default)
        {
            if (userId == Guid.Empty) throw new ArgumentException("userId required");
            if (quantity <= 0) throw new ArgumentException("quantity must be > 0");

            // Fetch product
            var products = await _productClient.GetProductsAsync(new[] { productId }, ct);
            if (!products.TryGetValue(productId, out var prod))
            {
                throw new InvalidOperationException($"Product {productId} not found");
            }

            if (prod.Stock < quantity)
            {
                throw new InvalidOperationException($"Product {prod.ProductId} stock insufficient");
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var oi = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = prod.ProductId,
                ProductName = prod.Name,
                PriceAtPurchase = prod.Price,
                Quantity = quantity
            };
            order.Items.Add(oi);

            order.Total = order.Items.Sum(i => i.PriceAtPurchase * i.Quantity);

            // Persist order (do NOT touch cart)
            var created = await _orderRepo.CreateOrderAsync(order, ct);

            return new CreateOrderResponse { OrderId = created.Id };
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(Guid id, CancellationToken ct = default)
        {
            var o = await _orderRepo.GetByIdAsync(id, ct);
            if (o == null) return null;
            return Map(o);
        }

        public async Task<OrderResponse[]> GetOrdersByUserAsync(Guid userId, CancellationToken ct = default)
        {
            var arr = await _orderRepo.GetByUserAsync(userId, ct);
            return arr.Select(Map).ToArray();
        }

        private static OrderResponse Map(Order o) =>
            new OrderResponse
            {
                Id = o.Id,
                UserId = o.UserId,
                CreatedAt = o.CreatedAt,
                Total = o.Total,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    PriceAtPurchase = i.PriceAtPurchase,
                    Quantity = i.Quantity
                }).ToList()
            };
    }
}