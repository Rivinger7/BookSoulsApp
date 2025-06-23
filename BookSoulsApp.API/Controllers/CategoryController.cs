using BookSoulsApp.Application.Models.Categories;
using BookSoulsApp.Application.ServiceInterfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookSoulsApp.API.Controllers;
[Route("api/categories")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
public class CategoryController(ICategoryService categoryService) : ControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] int pageIndex = 1, [FromQuery] int limit = 10)
    {
        var result = await _categoryService.GetCategoriesAsync(pageIndex, limit);
        return Ok(new { message = "Category retrieved successfully", result });
    }

    [AllowAnonymous, HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(string id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return Ok(new { message = $"Category with ID {id} retrieved successfully", result });
    }

    [Authorize(Roles = "Admin"), HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest createCategoryRequest)
    {
        await _categoryService.CreateCategoryAsync(createCategoryRequest);
        return Ok(new {Message = "Created category Successfully"});
    }

    [Authorize(Roles = "Admin"), HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(string id,[FromBody] UpdateCategoryRequest updateCategoryRequest)
    {
        await _categoryService.UpdateCategoryByIdAsync(id, updateCategoryRequest);
        return Ok(new { Message = "Updated category Successfully" });
    }

    [Authorize(Roles = "Admin"), HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        await _categoryService.DeleteCategoryByIdAsync(id);
        return Ok(new { Message = "Deleted category Successfully" });
    }
}
