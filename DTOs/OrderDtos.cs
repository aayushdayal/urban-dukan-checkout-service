using System;
using System.Collections.Generic;

namespace urban_dukan_checkout_service.DTOs
{
    public class CreateOrderResponse
    {
        public Guid OrderId { get; set; }
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal PriceAtPurchase { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderResponse
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Total { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
}