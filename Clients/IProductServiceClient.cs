using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace urban_dukan_checkout_service.Clients
{
    public class ProductInfo
    {
        public int id { get; set; }
        public string Title { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public interface IProductServiceClient
    {
        /// <summary>
        /// Gets product information for the given productIds. Throws if a product does not exist or HTTP call fails.
        /// </summary>
        Task<Dictionary<int, ProductInfo>> GetProductsAsync(IEnumerable<int> productIds, CancellationToken ct = default);
    }
}