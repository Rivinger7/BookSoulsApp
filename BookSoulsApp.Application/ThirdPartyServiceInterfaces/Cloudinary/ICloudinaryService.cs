using CloudinaryDotNet.Actions;
using BookSoulsApp.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace BookSoulsApp.Application.ThirdPartyServiceInterfaces.Cloudinary;

public interface ICloudinaryService
{
    public ImageUploadResult UploadImage(IFormFile imageFile, ImageTag imageTag, string rootFolder = "Avatar");
}
