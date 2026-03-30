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
    [Route("api/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _svc;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService svc, ILogger<OrderController> logger)
        {
            _svc = svc;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CancellationToken ct)
        {
            if (!TryGetUserId(out var userId) || userId == Guid.Empty) return Unauthorized();

            try
            {
                var res = await _svc.CreateOrderAsync(userId, ct);
                return CreatedAtAction(nameof(GetOrderById), new { id = res.OrderId }, res);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order");
                return Problem("Failed to create order");
            }
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous] // keep read access publicly if desired; adjust as needed
        public async Task<IActionResult> GetOrderById(Guid id, CancellationToken ct)
        {
            var order = await _svc.GetOrderByIdAsync(id, ct);
            if (order == null) return NotFound();
            return Ok(order);
        }

        // returns orders for the authenticated user
        [HttpGet("me")]
        public async Task<IActionResult> GetOrdersByUser(CancellationToken ct)
        {
            if (!TryGetUserId(out var userId) || userId == Guid.Empty) return Unauthorized();
            var orders = await _svc.GetOrdersByUserAsync(userId, ct);
            return Ok(orders);
        }

        private bool TryGetUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var claim = User.FindFirst("sub") ?? User.FindFirst("user_id") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null) return false;
            return Guid.TryParse(claim.Value, out userId);
        }
    }
}