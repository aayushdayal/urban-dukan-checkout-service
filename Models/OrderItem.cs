using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace urban_dukan_checkout_service.Models
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(Order))]
        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }

        public string ProductName { get; set; } = "";

        public decimal PriceAtPurchase { get; set; }

        public int Quantity { get; set; }

        public Order? Order { get; set; }
    }
}