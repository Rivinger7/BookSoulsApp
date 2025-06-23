using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.Models.Publishers;

namespace BookSoulsApp.Application.ServiceInterfaces;
public interface IPublisherService
{
    Task CreatePublisherAsync(CreatePublisherRequest createPublisherRequest);
    Task DeletePublisherByIdAsync(string id);
    Task<PublisherResponse> GetPublisherByIdAsync(string id);
    Task<PaginatedResult<PublisherResponse>> GetPublishersAsync(int pageIndex = 1, int limit = 10);
    Task UpdatePublisherByIdAsync(string id, UpdatePublisherRequest updatePublisherRequest);
}
