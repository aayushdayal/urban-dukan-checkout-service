using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using urban_dukan_checkout_service.DTOs;
using urban_dukan_checkout_service.Services;

namespace urban_dukan_checkout_service.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize]
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
        public async Task<IActionResult> GetCart(CancellationToken ct)
        {
            if (!TryGetUserId(out var userId) || userId == 0) return Unauthorized();
            var cart = await _service.GetCartAsync(userId, ct);
            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken ct)
        {
            if (!TryGetUserId(out var userId) || userId == 0) return Unauthorized();

            // set user from token (frontend no longer provides it)
            request.UserId = userId;

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
            if (!TryGetUserId(out var userId) || userId == 0) return Unauthorized();

            request.UserId = userId;

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

        // productId is an integer
        [HttpDelete("items/{productId:int}")]
        public async Task<IActionResult> DeleteItem(int productId, CancellationToken ct)
        {
            if (!TryGetUserId(out var userId) || userId == 0) return Unauthorized();

            await _service.RemoveItemAsync(userId, productId, ct);
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart(CancellationToken ct)
        {
            if (!TryGetUserId(out var userId) || userId == 0) return Unauthorized();

            await _service.ClearCartAsync(userId, ct);
            return NoContent();
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var claim = User.FindFirst("sub") ?? User.FindFirst("user_id") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null) return false;
            return int.TryParse(claim.Value, out userId);
        }
    }
}