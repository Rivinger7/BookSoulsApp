using BookSoulsApp.Application.Models.Publishers;
using BookSoulsApp.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSoulsApp.API.Controllers;
[Route("api/publishers")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
public class PublisherController(IPublisherService publisherService) : ControllerBase
{
    private readonly IPublisherService _publisherService = publisherService;

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> GetPublishers([FromQuery] int pageIndex = 1, [FromQuery] int limit = 10)
    {
        var result = await _publisherService.GetPublishersAsync(pageIndex, limit);
        return Ok(new { message = "Publishers retrieved successfully", result });
    }

    [AllowAnonymous, HttpGet("{id}")]
    public async Task<IActionResult> GetPublisherById(string id)
    {
        var result = await _publisherService.GetPublisherByIdAsync(id);
        return Ok(new { message = $"Publisher with ID {id} retrieved successfully", result });
    }

    [Authorize(Roles = "Staff"), HttpPost]
    public async Task<IActionResult> CreatePublisher([FromBody] CreatePublisherRequest createPublisherRequest)
    {
        await _publisherService.CreatePublisherAsync(createPublisherRequest);
        return Ok(new { Message = "Created publisher Successfully" });
    }

    [Authorize(Roles = "Staff"), HttpPut("{id}")]
    public async Task<IActionResult> UpdatePublisher(string id, [FromBody] UpdatePublisherRequest updatePublisherRequest)
    {
        await _publisherService.UpdatePublisherByIdAsync(id, updatePublisherRequest);
        return Ok(new { Message = "Updated publisher Successfully" });
    }

    [Authorize(Roles = "Staff"), HttpDelete("{id}")]
    public async Task<IActionResult> DeletePublisher(string id)
    {
        await _publisherService.DeletePublisherByIdAsync(id);
        return Ok(new { Message = "Deleted publisher Successfully" });
    }
}
