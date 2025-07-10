using AutoMapper;
using BookSoulsApp.Application.Models.Books;
using BookSoulsApp.Application.Models.Pagination;
using BookSoulsApp.Application.ServiceInterfaces;
using BookSoulsApp.Application.ThirdPartyServiceInterfaces.Cloudinary;
using BookSoulsApp.Domain.Entities;
using BookSoulsApp.Domain.Enums;
using BookSoulsApp.Domain.Exceptions;
using BookSoulsApp.Domain.Utils;
using CloudinaryDotNet.Actions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace BookSoulsApp.Infrastructure.Services;
public class BookService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService) : IBookService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ICloudinaryService _cloudinaryService = cloudinaryService;

    public async Task<PaginatedResult<BookResponse>> GetBooksAsync(BookFilterRequest bookFilterRequest, int pageIndex = 1, int limit = 10)
    {
        IQueryable<Book> query = _unitOfWork.GetCollection<Book>().AsQueryable();

        if (!string.IsNullOrEmpty(bookFilterRequest.Title))
        {
            query = query.Where(b => b.Title.ToLower().Contains(bookFilterRequest.Title.ToLower()));
        }
        if (!string.IsNullOrEmpty(bookFilterRequest.Author))
        {
            query = query.Where(b => b.Author.ToLower().Contains(bookFilterRequest.Author.ToLower()));
        }
        if (!string.IsNullOrEmpty(bookFilterRequest.Isbn))
        {
            query = query.Where(b => b.Isbn == bookFilterRequest.Isbn);
        }
        if (!string.IsNullOrEmpty(bookFilterRequest.PublisherId))
        {
            query = query.Where(b => b.PublisherId == bookFilterRequest.PublisherId);
        }
        if (bookFilterRequest.CategoryIds is not null && bookFilterRequest.CategoryIds.Any())
        {
            query = query.Where(b => b.CategoryIds.Any(c => bookFilterRequest.CategoryIds.Contains(c)));
        }
        if (bookFilterRequest.ReleaseYear.HasValue)
        {
            query = query.Where(b => b.ReleaseYear == bookFilterRequest.ReleaseYear.Value);
        }
        if (bookFilterRequest.IsStricted.HasValue)
        {
            query = query.Where(b => b.IsStricted == bookFilterRequest.IsStricted.Value);
        }
        if (bookFilterRequest.MinPrice.HasValue)
        {
            query = query.Where(b => b.Price >= bookFilterRequest.MinPrice.Value);
        }
        if (bookFilterRequest.MaxPrice.HasValue)
        {
            query = query.Where(b => b.Price <= bookFilterRequest.MaxPrice.Value);
        }
        if (bookFilterRequest.MinStockQuantity.HasValue)
        {
            query = query.Where(b => b.Stock >= bookFilterRequest.MinStockQuantity.Value);
        }
        if (bookFilterRequest.MaxStockQuantity.HasValue)
        {
            query = query.Where(b => b.Stock <= bookFilterRequest.MaxStockQuantity.Value);
        }

        // Lọc các sách chưa bị xóa
        query = query.Where(b => !b.IsDeleted);

        // Phân trang và thực hiện truy vấn
        IEnumerable<Book> books = await query
            .Skip((pageIndex - 1) * limit)
            .Take(limit)
            .ToListAsync();

        // Chuyển đổi sang Response
        IEnumerable<BookResponse> bookResponses = _mapper.Map<IEnumerable<BookResponse>>(books);

        return new PaginatedResult<BookResponse>
        {
            Items = bookResponses,
            TotalCount = books.Count(),
        };
    }

    public async Task<BookResponse> GetBookByIdAsync(string bookId)
    {
        if (string.IsNullOrEmpty(bookId))
        {
            throw new BadRequestCustomException("Book ID cannot be null or empty.");
        }

        Book book = await _unitOfWork.GetCollection<Book>().Find(b => b.Id == bookId && !b.IsDeleted).FirstOrDefaultAsync() ?? throw new NotFoundCustomException($"Book with ID {bookId} not found.");

        return _mapper.Map<BookResponse>(book);
    }

    public async Task CreateBookAsync(CreateBookRequest createBookRequest)
    {
        // Upload ảnh lên Cloudinary
        string imageUrl = string.Empty;
        if (createBookRequest.Image is not null)
        {
            ImageUploadResult? result = _cloudinaryService.UploadImage(createBookRequest.Image, ImageTag.Book);

            imageUrl = result.SecureUrl.AbsoluteUri;
            if (string.IsNullOrEmpty(imageUrl))
            {
                imageUrl = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1750685876/pngtree-for-a-school-text-book-vector-png-image_15489235_i8q7ur.png";
            }
        }

        Book book = new()
        {
            Title = createBookRequest.Title,
            Author = createBookRequest.Author,
            Description = createBookRequest.Description,
            ReleaseYear = createBookRequest.ReleaseYear,
            IsStricted = createBookRequest.IsStricted,
            Price = createBookRequest.Price,
            Stock = createBookRequest.Stock,

            PublisherId = createBookRequest.PublisherId,
            CategoryIds = createBookRequest.CategoryIds,

            Image = imageUrl,

            CreatedAt = TimeControl.GetUtcPlus7Time()
        };

        await _unitOfWork.GetCollection<Book>().InsertOneAsync(book);
    }

    public async Task UpdateBookByIdAsync(string id, UpdateBookRequest updateBookRequest)
    {
        Book book = await _unitOfWork.GetCollection<Book>().Find(b => b.Id == id).FirstOrDefaultAsync() ?? throw new NotFoundCustomException("Book Not Found");

        UpdateDefinitionBuilder<Book> updateBuilder = Builders<Book>.Update;
        List<UpdateDefinition<Book>> updates = [];

        if (!string.IsNullOrEmpty(updateBookRequest.Title))
        {
            updates.Add(updateBuilder.Set(b => b.Title, updateBookRequest.Title));
        }
        if (!string.IsNullOrEmpty(updateBookRequest.Author))
        {
            updates.Add(updateBuilder.Set(b => b.Author, updateBookRequest.Author));
        }
        if (!string.IsNullOrEmpty(updateBookRequest.Description))
        {
            updates.Add(updateBuilder.Set(b => b.Description, updateBookRequest.Description));
        }
        if (!string.IsNullOrEmpty(updateBookRequest.PublisherId))
        {
            updates.Add(updateBuilder.Set(b => b.PublisherId, updateBookRequest.PublisherId));
        }
        if (updateBookRequest.CategoryIds.Count == 0)
        {
            updates.Add(updateBuilder.Set(b => b.CategoryIds, updateBookRequest.CategoryIds));
        }
        if (updateBookRequest.ReleaseYear is null)
        {
            updates.Add(updateBuilder.Set(b => b.ReleaseYear, updateBookRequest.ReleaseYear));
        }
        if (updateBookRequest.IsStricted is null)
        {
            updates.Add(updateBuilder.Set(b => b.IsStricted, updateBookRequest.IsStricted));
        }
        if (updateBookRequest.Price is null)
        {
            updates.Add(updateBuilder.Set(b => b.Price, updateBookRequest.Price));
        }
        if (updateBookRequest.Stock is null)
        {
            updates.Add(updateBuilder.Set(b => b.Stock, updateBookRequest.Stock));
        }


        if (updateBookRequest.Image is not null)
        {
            ImageUploadResult result = _cloudinaryService.UploadImage(updateBookRequest.Image, ImageTag.Book);
            string image = result.SecureUrl.AbsoluteUri;

            if (!string.IsNullOrEmpty(image))
            {
                updates.Add(updateBuilder.Set(b => b.Image, image));
            }
        }

        if (updates.Count == 0)
        {
            throw new BadRequestCustomException("Update failed");
        }

        UpdateDefinition<Book> updateDefinition = updateBuilder.Combine(updates);

        UpdateResult updateResult = await _unitOfWork.GetCollection<Book>()
            .UpdateOneAsync(b => b.Id == id, updateDefinition);

        if (updateResult.ModifiedCount == 0)
        {
            throw new NotFoundCustomException("Update failed");
        }
    }

    public async Task DeleteBookByIdAsync(string id)
    {
        // Mark the book as deleted
        UpdateResult updateResult = await _unitOfWork.GetCollection<Book>()
            .UpdateOneAsync(c => c.Id == id, Builders<Book>.Update.Set(c => c.IsDeleted, true));

        if (updateResult.ModifiedCount == 0)
        {
            throw new NotFoundCustomException("Delete failed");
        }
    }
}
