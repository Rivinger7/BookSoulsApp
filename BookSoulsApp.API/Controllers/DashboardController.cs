using BookSoulsApp.Application.Models.Orders;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSoulsApp.API.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
    public class DashboardController(IDashboardService dashboardService) : Controller
    {
        private readonly IDashboardService _dashboardService = dashboardService;

        [Authorize(Roles = "Admin, Staff"), HttpPost("/revenue")]
        //[AllowAnonymous, HttpPost("/revenue")]
        public async Task<IActionResult> GetRevenueByDay([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            var result = await _dashboardService.GetRevenueByDay(fromDate, toDate);
            return Ok(result);
        }
    }
}
