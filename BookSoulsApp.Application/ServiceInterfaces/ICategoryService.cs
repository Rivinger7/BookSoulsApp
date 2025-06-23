using BookSoulsApp.Application.Models.Categories;
using BookSoulsApp.Application.Models.Pagination;

namespace BookSoulsApp.Application.ServiceInterfaces;
public interface ICategoryService
{
    Task CreateCategoryAsync(CreateCategoryRequest createCategoryRequest);
    Task DeleteCategoryByIdAsync(string id);
    Task<PaginatedResult<CategoryResponse>> GetCategoriesAsync(int pageIndex = 1, int limit = 10);
    Task<CategoryResponse> GetCategoryByIdAsync(string id);
    Task UpdateCategoryByIdAsync(string id, UpdateCategoryRequest updateCategoryRequest);
}
