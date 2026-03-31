using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace urban_dukan_checkout_service.Clients
{
    public class ProductServiceClient : IProductServiceClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<ProductServiceClient> _logger;

        public ProductServiceClient(HttpClient http, ILogger<ProductServiceClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<Dictionary<int, ProductInfo>> GetProductsAsync(IEnumerable<int> productIds, CancellationToken ct = default)
        {
            var ids = productIds.Distinct().ToArray();
            var tasks = ids.Select(id => GetProductSingleAsync(id, ct)).ToArray();
            var results = await Task.WhenAll(tasks);

            return results.ToDictionary(r => r.id);
        }

        private async Task<ProductInfo> GetProductSingleAsync(int id, CancellationToken ct)
        {
            // Adjust path as per Product Service API. This assumes GET /api/products/{id}
            var url = $"api/products/{id}";
            HttpResponseMessage resp;
            try
            {
                resp = await _http.GetAsync(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Product service request failed for {ProductId}", id);
                throw new HttpRequestException($"Failed to reach Product service for {id}", ex);
            }

            if (!resp.IsSuccessStatusCode)
            {
                var text = await resp.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Product service returned {Status} for {ProductId}: {Text}", resp.StatusCode, id, text);
                throw new InvalidOperationException($"Product {id} not found or product service error");
            }

            var dto = await resp.Content.ReadFromJsonAsync<ProductInfo>(cancellationToken: ct);
            if (dto == null) throw new InvalidOperationException($"Product {id} returned invalid payload");
            return dto;
        }
    }
}