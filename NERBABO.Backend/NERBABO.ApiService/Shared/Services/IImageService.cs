using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.Services;

public interface IImageService
{
    Task<Result<string>> SaveImageAsync(IFormFile imageFile, string folderPath);
    Task<Result> DeleteImageAsync(string imagePath);
    Task<byte[]?> GetImageAsync(string imagePath);
    bool IsValidImageFile(IFormFile file);
}