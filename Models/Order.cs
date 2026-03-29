using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace urban_dukan_checkout_service.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public decimal Total { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}