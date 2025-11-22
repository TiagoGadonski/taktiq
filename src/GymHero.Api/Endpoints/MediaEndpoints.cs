using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Application.Services;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class MediaEndpoints
{
    public static void MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/media")
            .WithTags("Media")
            .RequireAuthorization();

        // Upload a file (image or video)
        group.MapPost("/upload", async (
            HttpRequest request,
            ClaimsPrincipal user,
            IFileStorageService fileStorageService,
            IVideoProcessingService videoProcessingService,
            IApplicationDbContext context,
            IConfiguration configuration,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            IFormFile? file = null;

            try
            {
                if (!request.HasFormContentType)
                {
                    logger.LogWarning("User {UserId} attempted file upload without multipart/form-data", userId);
                    return Results.BadRequest(new { message = "Request must be multipart/form-data" });
                }

                var form = await request.ReadFormAsync(cancellationToken);
                file = form.Files.FirstOrDefault();

                if (file == null || file.Length == 0)
                {
                    logger.LogWarning("User {UserId} attempted upload with no file", userId);
                    return Results.BadRequest(new { message = "No file uploaded" });
                }

                // Validate file size (max 100MB)
                const long maxFileSize = 100 * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    logger.LogWarning("User {UserId} attempted upload of oversized file: {Size}MB, Name: {FileName}",
                        userId, file.Length / 1024.0 / 1024.0, file.FileName);
                    return Results.BadRequest(new { message = "File size exceeds maximum limit of 100MB" });
                }

                // Validate file type (whitelist)
                var contentType = file.ContentType.ToLower();
                var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                var allowedVideoTypes = new[] { "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo", "video/webm" };
                var allAllowedTypes = allowedImageTypes.Concat(allowedVideoTypes).ToArray();

                if (!allAllowedTypes.Contains(contentType))
                {
                    logger.LogWarning("User {UserId} attempted upload of disallowed file type: {ContentType}, Name: {FileName}",
                        userId, contentType, file.FileName);
                    return Results.BadRequest(new { message = $"File type '{contentType}' is not allowed. Allowed types: images (JPEG, PNG, GIF, WebP) and videos (MP4, MPEG, QuickTime, AVI, WebM)" });
                }

                // Validate file extension matches content type
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4", ".mpeg", ".mov", ".avi", ".webm" };

                if (!validExtensions.Contains(fileExtension))
                {
                    logger.LogWarning("User {UserId} attempted upload with suspicious extension: {Extension}, Name: {FileName}",
                        userId, fileExtension, file.FileName);
                    return Results.BadRequest(new { message = "Invalid file extension" });
                }

                // Get optional parameters
                var usageContext = form["usageContext"].ToString();
                var entityIdStr = form["entityId"].ToString();
                Guid? entityId = string.IsNullOrEmpty(entityIdStr) ? null : Guid.Parse(entityIdStr);

                // Get compression settings from form or configuration
                var compressVideo = bool.Parse(form["compress"].ToString() ?? configuration["VideoCompression:AutoCompress"] ?? "false");
                var qualityStr = form["quality"].ToString() ?? configuration["VideoCompression:DefaultQuality"] ?? "Medium";

                // Determine media type from content type (use already validated contentType)
                var mediaType = contentType.StartsWith("image/") ? MediaType.Image : MediaType.Video;
                var containerName = mediaType == MediaType.Image ? "images" : "videos";

                // Compress video if enabled and it's a video file
                byte[]? compressedVideoData = null;
                VideoMetadata? compressionMetadata = null;
                long originalFileSize = file.Length;

                if (mediaType == MediaType.Video && compressVideo && configuration.GetValue<bool>("VideoCompression:Enabled"))
                {
                    try
                    {
                        var quality = Enum.Parse<VideoQuality>(qualityStr, ignoreCase: true);
                        var maxResolution = configuration.GetValue<int?>("VideoCompression:MaxResolution");

                        logger.LogInformation("Compressing video: {FileName}, Quality: {Quality}, MaxRes: {MaxRes}",
                            file.FileName, quality, maxResolution);

                        using (var videoStream = file.OpenReadStream())
                        {
                            var (videoData, metadata) = await videoProcessingService.CompressVideoAsync(
                                videoStream,
                                quality,
                                maxResolution
                            );

                            compressedVideoData = videoData;
                            compressionMetadata = metadata;
                        }

                        logger.LogInformation("Video compressed: {OriginalSize}MB -> {CompressedSize}MB",
                            originalFileSize / 1024.0 / 1024.0,
                            compressedVideoData.Length / 1024.0 / 1024.0);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Video compression failed, uploading original file");
                        compressedVideoData = null; // Fallback to original file
                    }
                }

                // Upload file to blob storage (compressed or original)
                string fileUrl;
                long uploadedFileSize;

                if (compressedVideoData != null)
                {
                    // Upload compressed video
                    using (var compressedStream = new MemoryStream(compressedVideoData))
                    {
                        var compressedFileName = Path.GetFileNameWithoutExtension(file.FileName) + "_compressed.mp4";
                        fileUrl = await fileStorageService.UploadFileAsync(
                            compressedFileName,
                            compressedStream,
                            "video/mp4",
                            containerName
                        );
                        uploadedFileSize = compressedVideoData.Length;
                    }
                }
                else
                {
                    // Upload original file
                    using (var stream = file.OpenReadStream())
                    {
                        fileUrl = await fileStorageService.UploadFileAsync(
                            file.FileName,
                            stream,
                            contentType,
                            containerName
                        );
                        uploadedFileSize = file.Length;
                    }
                }

                // Process video if this is a video file
                string? thumbnailUrl = null;
                double? durationSeconds = null;
                int? width = null;
                int? height = null;

                if (mediaType == MediaType.Video)
                {
                    try
                    {
                        // Use compression metadata if available, otherwise extract from original
                        if (compressionMetadata != null)
                        {
                            durationSeconds = compressionMetadata.DurationSeconds;
                            width = compressionMetadata.Width;
                            height = compressionMetadata.Height;
                            logger.LogInformation("Using compressed video metadata: {Duration}s, {Width}x{Height}",
                                durationSeconds, width, height);
                        }
                        else
                        {
                            // Extract video metadata from original
                            using (var metadataStream = file.OpenReadStream())
                            {
                                var metadata = await videoProcessingService.GetVideoMetadataAsync(metadataStream);
                                durationSeconds = metadata.DurationSeconds;
                                width = metadata.Width;
                                height = metadata.Height;
                                logger.LogInformation("Video metadata extracted: {Duration}s, {Width}x{Height}",
                                    durationSeconds, width, height);
                            }
                        }

                        // Generate and upload thumbnail
                        using (var thumbnailStream = file.OpenReadStream())
                        {
                            var thumbnailBytes = await videoProcessingService.GenerateThumbnailAsync(thumbnailStream);

                            // Upload thumbnail to blob storage
                            using (var thumbStream = new MemoryStream(thumbnailBytes))
                            {
                                var thumbnailFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_thumb.jpg";
                                thumbnailUrl = await fileStorageService.UploadFileAsync(
                                    thumbnailFileName,
                                    thumbStream,
                                    "image/jpeg",
                                    "thumbnails"
                                );
                                logger.LogInformation("Video thumbnail generated and uploaded: {ThumbUrl}", thumbnailUrl);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to process video, continuing without thumbnail");
                        // Continue without thumbnail/metadata if processing fails
                    }
                }

                // Create media record in database
                var media = new Media
                {
                    FileName = file.FileName,
                    FileUrl = fileUrl,
                    ContentType = contentType,
                    FileSizeBytes = uploadedFileSize, // Use compressed size if compression was applied
                    MediaType = mediaType,
                    ContainerName = containerName,
                    UploadedBy = userId,
                    UsageContext = usageContext,
                    EntityId = entityId,
                    ThumbnailUrl = thumbnailUrl,
                    DurationSeconds = durationSeconds,
                    Width = width,
                    Height = height
                };

                context.Medias.Add(media);
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Media uploaded successfully: User {UserId}, File: {FileName}, Type: {MediaType}, Size: {Size}MB, URL: {FileUrl}",
                    userId, file.FileName, mediaType, uploadedFileSize / 1024.0 / 1024.0, fileUrl);

                var response = new MediaUploadResponse(
                    media.Id,
                    media.FileName,
                    media.FileUrl,
                    media.ContentType,
                    media.FileSizeBytes,
                    media.MediaType.ToString(),
                    media.ThumbnailUrl,
                    media.DurationSeconds,
                    media.Width,
                    media.Height,
                    media.CreatedAt
                );

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "File upload failed for user {UserId}, File: {FileName}", userId, file?.FileName ?? "unknown");
                return Results.Problem(
                    title: "File upload failed",
                    detail: "Unable to upload file. Please try again.",
                    statusCode: 500
                );
            }
        })
        .WithName("UploadMedia")
        .WithSummary("Upload an image or video file")
        .DisableAntiforgery(); // Required for file uploads

        // Get media by ID
        group.MapGet("/{mediaId:guid}", async (
            Guid mediaId,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var media = await context.Medias
                .Where(m => m.Id == mediaId && !m.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (media == null)
            {
                return Results.NotFound(new { message = "Media not found" });
            }

            var response = new MediaResponse(
                media.Id,
                media.FileName,
                media.FileUrl,
                media.ContentType,
                media.FileSizeBytes,
                media.MediaType.ToString(),
                media.ContainerName,
                media.UploadedBy,
                media.UsageContext,
                media.EntityId,
                media.ThumbnailUrl,
                media.DurationSeconds,
                media.Width,
                media.Height,
                media.CreatedAt
            );

            return Results.Ok(response);
        })
        .WithName("GetMedia")
        .WithSummary("Get media file details by ID");

        // Get user's media files
        group.MapGet("/my", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            [FromQuery] string? mediaType,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var query = context.Medias
                .Where(m => m.UploadedBy == userId && !m.IsDeleted);

            if (!string.IsNullOrEmpty(mediaType) && Enum.TryParse<MediaType>(mediaType, true, out var type))
            {
                query = query.Where(m => m.MediaType == type);
            }

            var mediaList = await query
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MediaSummaryResponse(
                    m.Id,
                    m.FileName,
                    m.FileUrl,
                    m.ContentType,
                    m.MediaType.ToString(),
                    m.ThumbnailUrl,
                    m.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return Results.Ok(mediaList);
        })
        .WithName("GetMyMedia")
        .WithSummary("Get all media files uploaded by the authenticated user");

        // Delete media
        group.MapDelete("/{mediaId:guid}", async (
            Guid mediaId,
            ClaimsPrincipal user,
            IFileStorageService fileStorageService,
            IApplicationDbContext context,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var media = await context.Medias
                .Where(m => m.Id == mediaId && m.UploadedBy == userId && !m.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (media == null)
            {
                logger.LogWarning("User {UserId} attempted to delete non-existent or unauthorized media: {MediaId}",
                    userId, mediaId);
                return Results.NotFound(new { message = "Media not found" });
            }

            var fileName = media.FileName;
            var fileUrl = media.FileUrl;

            // Soft delete in database
            media.IsDeleted = true;
            media.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Media deleted: User {UserId}, MediaId: {MediaId}, File: {FileName}",
                userId, mediaId, fileName);

            // Delete from blob storage (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await fileStorageService.DeleteFileAsync(fileUrl);
                    logger.LogInformation("Blob storage file deleted: {FileUrl}", fileUrl);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to delete blob storage file: {FileUrl}", fileUrl);
                }
            });

            return Results.NoContent();
        })
        .WithName("DeleteMedia")
        .WithSummary("Delete a media file");
    }
}
