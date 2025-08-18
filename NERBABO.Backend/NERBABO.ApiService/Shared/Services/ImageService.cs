using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.Services;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageService> _logger;
    private readonly string _imageStoragePath;
    private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".bmp"];
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    public ImageService(IWebHostEnvironment environment, ILogger<ImageService> logger)
    {
        _environment = environment;
        _logger = logger;
        
        // Use wwwroot/uploads for web-accessible files
        _imageStoragePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "images");
        Directory.CreateDirectory(_imageStoragePath);
    }

    public async Task<Result<string>> SaveImageAsync(IFormFile imageFile, string folderPath)
    {
        try
        {
            if (!IsValidImageFile(imageFile))
            {
                return Result<string>.Fail("Arquivo inválido", "O arquivo deve ser uma imagem válida (JPG, PNG, GIF, BMP) e menor que 5MB.");
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var fullFolderPath = Path.Combine(_imageStoragePath, folderPath);
            Directory.CreateDirectory(fullFolderPath);
            
            var filePath = Path.Combine(fullFolderPath, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            
            var relativePath = Path.Combine(folderPath, fileName);
            _logger.LogInformation("Image saved successfully at {RelativePath}", relativePath);
            
            return Result<string>.Ok(relativePath, "Imagem salva", "Imagem salva com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving image file");
            return Result<string>.Fail("Erro interno", "Erro ao salvar a imagem.");
        }
    }

    public async Task<Result> DeleteImageAsync(string imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return Result.Ok("Nenhuma imagem", "Não há imagem para remover.");
            }

            var fullPath = Path.Combine(_imageStoragePath, imagePath);
            
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation("Image deleted successfully at {ImagePath}", imagePath);
            }
            
            return Result.Ok("Imagem removida", "Imagem removida com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image file at {ImagePath}", imagePath);
            return Result.Fail("Erro interno", "Erro ao deletar a imagem.");
        }
    }

    public async Task<byte[]?> GetImageAsync(string imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath))
                return null;

            var fullPath = Path.Combine(_imageStoragePath, imagePath);
            
            if (!File.Exists(fullPath))
                return null;

            return await File.ReadAllBytesAsync(fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading image file at {ImagePath}", imagePath);
            return null;
        }
    }

    public bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > _maxFileSize)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return false;

        // Basic content type check
        if (!file.ContentType.StartsWith("image/"))
            return false;

        return true;
    }
}