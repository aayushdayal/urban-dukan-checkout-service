using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using urban_dukan_checkout_service.DTOs;
using urban_dukan_checkout_service.Services;

namespace urban_dukan_checkout_service.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _service;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService service, ILogger<CartController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart([FromQuery] Guid userId, CancellationToken ct)
        {
            if (userId == Guid.Empty) return BadRequest("userId is required");
            var cart = await _service.GetCartAsync(userId, ct);
            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _service.AddItemAsync(request, ct);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item");
                return Problem("Failed to add item");
            }
        }

        [HttpPut("items")]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateCartItemRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _service.UpdateItemAsync(request, ct);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update item");
                return Problem("Failed to update item");
            }
        }

        [HttpDelete("items/{productId:guid}")]
        public async Task<IActionResult> DeleteItem([FromQuery] Guid userId, Guid productId, CancellationToken ct)
        {
            if (userId == Guid.Empty) return BadRequest("userId is required");
            await _service.RemoveItemAsync(userId, productId, ct);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart([FromQuery] Guid userId, CancellationToken ct)
        {
            if (userId == Guid.Empty) return BadRequest("userId is required");
            await _service.ClearCartAsync(userId, ct);
            return NoContent();
        }
    }
}