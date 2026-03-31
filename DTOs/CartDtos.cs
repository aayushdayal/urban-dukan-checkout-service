using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace urban_dukan_checkout_service.DTOs
{
    public class AddCartItemRequest
    {
        // UserId is now set server-side from the token; keep as int.
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequest
    {
        // UserId is now set server-side from the token; keep as int.
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    // DTO for Buy Now action (UI sends product + quantity)
    public class BuyNowRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartResponse
    {
        public int UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}