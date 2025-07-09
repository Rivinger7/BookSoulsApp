using BookSoulsApp.Application.Models.Orders;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSoulsApp.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    public class OrderController(IOrderService orderService) : Controller
    {
        private readonly IOrderService _orderService = orderService;

        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] OrderFilterRequest req, [FromQuery] int pageIndex = 1, [FromQuery] int limit = 10)
        {
            var result = await _orderService.GetOrdersAsync(req, pageIndex, limit);
            return Ok(new { message = "Orders retrieved successfully", result });
        }

        [AllowAnonymous, HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return Ok(new { message = $"Order with ID {id} retrieved successfully", result });
        }

        [Authorize(Roles = "Customer"), HttpPost]
        //[AllowAnonymous, HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest req)
        {
            string checkout = await _orderService.CreateOrder(req);
            return Ok(checkout);
        }

        [Authorize(Roles = "Customer, Staff"), HttpPost("{id}/cancel")]
        //[AllowAnonymous, HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(string id, string cancelReason)
        {
            await _orderService.CancelOrder(id, cancelReason);
            return Ok(new { Message = "Cancel Order Successfully" });
        }

        [Authorize(Roles = "Staff"), HttpPost("{id}/status-change")]
        //[AllowAnonymous, HttpPost("{id}/status-change")]
        public async Task<IActionResult> ChangeOrderStatus(string id, OrderStatus status)
        {
            await _orderService.ChangeOrderStatus(id, status);
            return Ok(new { Message = "Change Order Status Successfully" });
        }
    }
}
