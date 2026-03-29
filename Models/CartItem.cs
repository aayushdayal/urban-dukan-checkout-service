using System;

namespace urban_dukan_checkout_service.Models
{
    public class CartItem
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}