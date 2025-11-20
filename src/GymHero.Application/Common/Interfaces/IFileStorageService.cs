namespace GymHero.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to blob storage
    /// </summary>
    /// <param name="fileName">The name of the file</param>
    /// <param name="fileStream">The file stream</param>
    /// <param name="contentType">The content type (e.g., image/jpeg, video/mp4)</param>
    /// <param name="containerName">The container name (e.g., "images", "videos")</param>
    /// <returns>The URL of the uploaded file</returns>
    Task<string> UploadFileAsync(string fileName, Stream fileStream, string contentType, string containerName = "media");

    /// <summary>
    /// Deletes a file from blob storage
    /// </summary>
    /// <param name="fileUrl">The URL of the file to delete</param>
    Task DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Gets a SAS token URL for temporary access to a file
    /// </summary>
    /// <param name="fileUrl">The URL of the file</param>
    /// <param name="expirationMinutes">How long the token should be valid (default 60 minutes)</param>
    /// <returns>URL with SAS token</returns>
    Task<string> GetSasTokenUrlAsync(string fileUrl, int expirationMinutes = 60);
}
