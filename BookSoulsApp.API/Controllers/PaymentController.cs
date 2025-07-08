using BookSoulsApp.Application.ThirdPartyServiceInterfaces.Payment;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using System.Text.Json;

namespace BookSoulsApp.API.Controllers
{
    [Route("api/payment")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    public class PaymentController(IPayosService payosService) : Controller
    {
        private readonly IPayosService _payosService = payosService;

        [AllowAnonymous, HttpPost("{orderId}/checkout-url")]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            var result = await _payosService.CreatePaymentLinkRequest(orderId);
            return Ok(result);
        }

        [AllowAnonymous, HttpGet("payment-link")]
        public async Task<IActionResult> GetPaymentLinkInfo(long paymentCode)
        {
            try
            {
                var result = await _payosService.GetPaymentLinkInfo(paymentCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous, HttpPost("webhook-handle")]
        public async Task<IActionResult> HandleWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            string rawBody = await reader.ReadToEndAsync();
            Console.WriteLine("Webhook raw body: " + rawBody);

            try
            {
                // Deserialize JSON vào đúng kiểu WebhookType từ SDK
                WebhookType? webhookType = JsonSerializer.Deserialize<WebhookType>(rawBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // cho phép JSON camelCase
                });
                if (webhookType == null)
                {
                    return BadRequest("Dữ liệu webhook không hợp lệ hoặc null!");
                }
                await _payosService.HandleWebhook(webhookType);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi xử lý webhook: " + ex.Message);
                return BadRequest("Invalid webhook");
            }
        }

        [AllowAnonymous, HttpPost("webhook-confirm")]
        public async Task<IActionResult> ConfirmWebhook(string url)
        {
            try
            {
                string result = await _payosService.ConfirmWebhook(url);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi confirm webhook: " + ex.Message);
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }
    }
}
