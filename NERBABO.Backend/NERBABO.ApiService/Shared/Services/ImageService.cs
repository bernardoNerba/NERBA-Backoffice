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

        // Ensure storage directory exists
        Directory.CreateDirectory(_imageStoragePath);
    }

    public async Task<Result<string>> SaveImageAsync(IFormFile imageFile, string folderPath)
    {
        try
        {
            // check image type and size
            if (!IsValidImageFile(imageFile))
            {
                _logger.LogWarning("Invalid image type or size.");
                return Result<string>
                    .Fail("Arquivo inválido",
                    "O arquivo deve ser uma imagem válida (JPG, PNG, GIF, BMP) e menor que 5MB.");
            }

            // determine a name and path to store the image
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var fullFolderPath = Path.Combine(_imageStoragePath, folderPath);

            // create the directory
            Directory.CreateDirectory(fullFolderPath);
            
            // get the full path for the file
            var filePath = Path.Combine(fullFolderPath, fileName);
            
            // copy the image to the path
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }
            
            // get the relative path
            var relativePath = Path.Combine(folderPath, fileName);
            _logger.LogInformation("Image saved successfully at {RelativePath}", relativePath);
            
            return Result<string>
                .Ok(relativePath, "Imagem guardada", "Imagem guardada com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving image file");
            return Result<string>
                .Fail("Erro interno", "Erro ao guardar a imagem.");
        }
    }

    public async Task<Result> DeleteImageAsync(string imagePath)
    {
        try
        {
            // check if there is image on the provided path
            if (string.IsNullOrEmpty(imagePath))
            {
                _logger.LogInformation("No image in the path provided {path}", imagePath);
                return Result.Ok("Nenhuma imagem", "Não há imagem para remover.");
            }

            // get the full path
            var fullPath = Path.Combine(_imageStoragePath, imagePath);
            
            // check if the file exists so that the file can be deleted
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
            // check if there is image on the provided path
            if (string.IsNullOrEmpty(imagePath))
                return null;

            // get the full path
            var fullPath = Path.Combine(_imageStoragePath, imagePath);
            
            // check if the file exists so the file content can be retrieved
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
        // check if the file provided has content
        if (file is null || file.Length == 0)
        {
            _logger.LogWarning("File provided invalid.");
            return false;
        }

        // check if the file provided has correct size
        if (file.Length > _maxFileSize)
        {
            _logger.LogWarning("File provided has invalid size.");
            return false;
        }

        // check if the extension of the file is allowed
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            _logger.LogWarning("File provided has invalid extension {extension}.", extension);
            return false;
        }

        // Basic content type check
        if (!file.ContentType.StartsWith("image/"))
        {
            _logger.LogWarning("The provided file content type is invalid, should start with 'image/'");
            return false;
        }

        return true;
    }
}