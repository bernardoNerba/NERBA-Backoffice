using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.Services;

/// <summary>
/// Service for managing image file operations including upload, storage, retrieval, and validation.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Saves an uploaded image file to the specified folder path.
    /// Generates a unique filename and stores the file in the configured storage location.
    /// </summary>
    /// <param name="imageFile">The image file to save.</param>
    /// <param name="folderPath">The folder path where the image should be stored.</param>
    /// <returns>A result containing the saved image file path if successful.</returns>
    Task<Result<string>> SaveImageAsync(IFormFile imageFile, string folderPath);

    /// <summary>
    /// Deletes an image file from storage.
    /// </summary>
    /// <param name="imagePath">The path of the image file to delete.</param>
    /// <returns>A result indicating success or failure of the deletion operation.</returns>
    Task<Result> DeleteImageAsync(string imagePath);

    /// <summary>
    /// Retrieves the binary content of an image file.
    /// </summary>
    /// <param name="imagePath">The path of the image file to retrieve.</param>
    /// <returns>The image content as byte array, or null if the file doesn't exist.</returns>
    Task<byte[]?> GetImageAsync(string imagePath);

    /// <summary>
    /// Validates whether the uploaded file is a valid image format.
    /// Checks file extension and content type to ensure it's an acceptable image.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <returns>True if the file is a valid image format, false otherwise.</returns>
    bool IsValidImageFile(IFormFile file);
}