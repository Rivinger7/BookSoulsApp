using AutoMapper;
using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.Models.Publishers;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Exceptions;
using BookSoulsApp.Domain.Utils;
using MongoDB.Driver;

namespace BookSoulsApp.Infrastructure.Services;
public class PublisherService(IUnitOfWork unitOfWork, IMapper mapper) : IPublisherService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<PaginatedResult<PublisherResponse>> GetPublishersAsync(int pageIndex = 1, int limit = 10)
    {
        IEnumerable<Publisher> publishers = await _unitOfWork.GetCollection<Publisher>()
            .Find(p => p.IsDeleted == false)
            .Skip((pageIndex - 1) * limit).Limit(limit)
            .ToListAsync();

        IEnumerable<PublisherResponse> publisherResponses = _mapper.Map<IEnumerable<PublisherResponse>>(publishers);

        return new PaginatedResult<PublisherResponse>
        {
            Items = publisherResponses,
            TotalCount = publisherResponses.Count()
        };
    }

    public async Task<PublisherResponse> GetPublisherByIdAsync(string id)
    {
        Publisher publisher = await _unitOfWork.GetCollection<Publisher>()
            .Find(p => p.Id == id && p.IsDeleted == false)
            .FirstOrDefaultAsync() ?? throw new NotFoundCustomException($"Not found any publisher with ID {id}");

        return _mapper.Map<PublisherResponse>(publisher);
    }

    public async Task CreatePublisherAsync(CreatePublisherRequest createPublisherRequest)
    {
        Publisher publisher = new()
        {
            Name = createPublisherRequest.Name,
            Description = createPublisherRequest.Description,
            CreatedAt = TimeControl.GetUtcPlus7Time(), // Assuming you want to use UTC time
        };

        await _unitOfWork.GetCollection<Publisher>().InsertOneAsync(publisher);
    }

    public async Task UpdatePublisherByIdAsync(string id, UpdatePublisherRequest updatePublisherRequest)
    {
        // Check if the publisher exists
        Publisher publisher = await _unitOfWork.GetCollection<Publisher>()
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Publisher Not Found");

        // Build update definition dynamically
        UpdateDefinitionBuilder<Publisher> updateDefinitionBuilder = Builders<Publisher>.Update;
        List<UpdateDefinition<Publisher>> updates = [];

        if (!string.IsNullOrEmpty(updatePublisherRequest.Name))
        {
            updates.Add(updateDefinitionBuilder.Set(p => p.Name, updatePublisherRequest.Name));
        }
        if (!string.IsNullOrEmpty(updatePublisherRequest.Description))
        {
            updates.Add(updateDefinitionBuilder.Set(p => p.Description, updatePublisherRequest.Description));
        }

        UpdateDefinition<Publisher> updateDefinition = updateDefinitionBuilder.Combine(updates);
        updateDefinition = updateDefinitionBuilder.Combine(updates)
            .Set(p => p.UpdatedAt, TimeControl.GetUtcPlus7Time());

        UpdateResult updateResult = await _unitOfWork.GetCollection<Publisher>().UpdateOneAsync(p => p.Id == id, updateDefinition);

        if (updateResult.ModifiedCount == 0)
        {
            throw new BadRequestCustomException("Update failed");
        }
    }

    public async Task DeletePublisherByIdAsync(string id)
    {
        // Mark as deleted
        UpdateDefinition<Publisher> updateDefinition = Builders<Publisher>.Update
            .Set(p => p.IsDeleted, true);

        UpdateResult updateResult = await _unitOfWork.GetCollection<Publisher>().UpdateOneAsync(p => p.Id == id, updateDefinition);
        if (updateResult.ModifiedCount == 0)
        {
            throw new BadRequestCustomException("Delete failed");
        }
    }
}
