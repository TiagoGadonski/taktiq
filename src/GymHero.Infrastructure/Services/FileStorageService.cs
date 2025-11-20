using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using GymHero.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GymHero.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _accountName;

    public FileStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureStorage");

        // If no connection string is provided, use development storage (Azurite)
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "UseDevelopmentStorage=true";
        }

        _blobServiceClient = new BlobServiceClient(connectionString);

        // Extract account name from connection string for URL construction
        var accountNameMatch = connectionString.Contains("AccountName=")
            ? connectionString.Split("AccountName=")[1].Split(';')[0]
            : "devstoreaccount1"; // Default for Azurite

        _accountName = accountNameMatch;
    }

    public async Task<string> UploadFileAsync(
        string fileName,
        Stream fileStream,
        string contentType,
        string containerName = "media")
    {
        // Create container if it doesn't exist
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        // Generate a unique file name to avoid collisions
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var blobClient = containerClient.GetBlobClient(uniqueFileName);

        // Upload the file
        var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders
        });

        // Return the URL
        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var pathParts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathParts.Length < 2)
            {
                throw new ArgumentException("Invalid file URL format");
            }

            var containerName = pathParts[0];
            var blobName = string.Join('/', pathParts.Skip(1));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception)
        {
            // Silently fail if blob doesn't exist or can't be deleted
            // This prevents errors when deleting already-deleted files
        }
    }

    public async Task<string> GetSasTokenUrlAsync(string fileUrl, int expirationMinutes = 60)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var pathParts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathParts.Length < 2)
            {
                return fileUrl; // Return original URL if can't parse
            }

            var containerName = pathParts[0];
            var blobName = string.Join('/', pathParts.Skip(1));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check if the blob exists
            if (!await blobClient.ExistsAsync())
            {
                return fileUrl;
            }

            // Generate SAS token
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasToken = blobClient.GenerateSasUri(sasBuilder);
            return sasToken.ToString();
        }
        catch (Exception)
        {
            // Return original URL if SAS generation fails
            return fileUrl;
        }
    }
}
