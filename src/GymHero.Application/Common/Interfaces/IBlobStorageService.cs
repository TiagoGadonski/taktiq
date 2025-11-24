namespace GymHero.Application.Common.Interfaces;

public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a file to blob storage and returns the URL
    /// </summary>
    /// <param name="stream">The file stream to upload</param>
    /// <param name="fileName">The desired file name</param>
    /// <param name="contentType">The MIME type of the file</param>
    /// <param name="containerName">The blob container name (e.g., "profile-pictures")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The public URL of the uploaded blob</returns>
    Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from blob storage
    /// </summary>
    /// <param name="blobUrl">The full URL or blob name to delete</param>
    /// <param name="containerName">The blob container name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(
        string blobUrl,
        string containerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a blob exists
    /// </summary>
    /// <param name="blobUrl">The full URL or blob name</param>
    /// <param name="containerName">The blob container name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> ExistsAsync(
        string blobUrl,
        string containerName,
        CancellationToken cancellationToken = default);
}
