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
    [Route("api/orders")]
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
        public async Task<IActionResult> CreateOrder([FromQuery] Guid userId, CancellationToken ct)
        {
            if (userId == Guid.Empty) return BadRequest("userId is required");
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
        public async Task<IActionResult> GetOrderById(Guid id, CancellationToken ct)
        {
            var order = await _svc.GetOrderByIdAsync(id, ct);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetOrdersByUser(Guid userId, CancellationToken ct)
        {
            var orders = await _svc.GetOrdersByUserAsync(userId, ct);
            return Ok(orders);
        }
    }
}