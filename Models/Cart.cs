using System;
using System.Collections.Generic;

namespace urban_dukan_checkout_service.Models
{
    public class Cart
    {
        public Guid UserId { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}