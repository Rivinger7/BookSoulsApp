using AutoMapper;
using BookSoulsApp.Application.Models.Categories;
using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Exceptions;
using BookSoulsApp.Domain.Utils;
using MongoDB.Driver;

namespace BookSoulsApp.Infrastructure.Services;
public class CategoryService(IUnitOfWork unitOfWork, IMapper mapper) : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<PaginatedResult<CategoryResponse>> GetCategoriesAsync(int pageIndex = 1, int limit = 10)
    {
        IEnumerable<Category> categories = await _unitOfWork.GetCollection<Category>()
            .Find(c => c.IsDeleted == false)
            .Skip((pageIndex - 1) * limit).Limit(limit)
            .ToListAsync();

        IEnumerable<CategoryResponse> categoryResponses = _mapper.Map<IEnumerable<CategoryResponse>>(categories);

        long totalCount = await _unitOfWork.GetCollection<Category>()
            .CountDocumentsAsync(c => c.IsDeleted == false);

        return new PaginatedResult<CategoryResponse>
        {
            Items = categoryResponses,
            TotalCount = totalCount
        };
    }

    public async Task<CategoryResponse> GetCategoryByIdAsync(string id)
    {
        Category category = await _unitOfWork.GetCollection<Category>()
            .Find(c => c.Id == id && c.IsDeleted == false)
            .FirstOrDefaultAsync() ?? throw new NotFoundCustomException($"Not found any category with ID {id}");

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task CreateCategoryAsync(CreateCategoryRequest createCategoryRequest)
    {
        Category category = new()
        {
            Name = createCategoryRequest.Name,
            Description = createCategoryRequest.Description,
            CreatedAt = TimeControl.GetUtcPlus7Time(),
        };

        await _unitOfWork.GetCollection<Category>().InsertOneAsync(category);
    }

    public async Task UpdateCategoryByIdAsync(string id, UpdateCategoryRequest updateCategoryRequest)
    {
        // Check if the category exists
        Category category = await _unitOfWork.GetCollection<Category>()
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Category Not Found");

        // Build update definition dynamically
        UpdateDefinitionBuilder<Category> updateBuilder = Builders<Category>.Update;
        List<UpdateDefinition<Category>> updates = [];

        if (!string.IsNullOrEmpty(updateCategoryRequest.Name))
        {
            updates.Add(updateBuilder.Set(c => c.Name, updateCategoryRequest.Name));
        }
        if (!string.IsNullOrEmpty(updateCategoryRequest.Description))
        {
            updates.Add(updateBuilder.Set(c => c.Description, updateCategoryRequest.Description));
        }

        updates.Add(updateBuilder.Set(c => c.UpdatedAt, TimeControl.GetUtcPlus7Time()));

        UpdateDefinition<Category> updateDefinition = updateBuilder.Combine(updates);
        updateDefinition = updateBuilder.Combine(updates)
            .Set(p => p.UpdatedAt, TimeControl.GetUtcPlus7Time());

        // Perform the update
        UpdateResult updateResult = await _unitOfWork.GetCollection<Category>()
            .UpdateOneAsync(c => c.Id == id, updateDefinition);

        if (updateResult.ModifiedCount == 0)
        {
            throw new NotFoundCustomException("No Changes Made");
        }
    }

    public async Task DeleteCategoryByIdAsync(string id)
    {
        // Mark the category as deleted
        UpdateResult updateResult = await _unitOfWork.GetCollection<Category>()
            .UpdateOneAsync(c => c.Id == id, Builders<Category>.Update.Set(c => c.IsDeleted, true));

        if (updateResult.ModifiedCount == 0)
        {
            throw new NotFoundCustomException("Update failed");
        }
    }
}
