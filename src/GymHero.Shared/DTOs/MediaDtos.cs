namespace GymHero.Shared.DTOs;

/// <summary>
/// Response DTO for uploaded media
/// </summary>
public record MediaUploadResponse(
    Guid Id,
    string FileName,
    string FileUrl,
    string ContentType,
    long FileSizeBytes,
    string MediaType,
    string? ThumbnailUrl,
    double? DurationSeconds,
    int? Width,
    int? Height,
    DateTime CreatedAt
);

/// <summary>
/// Response DTO for media details
/// </summary>
public record MediaResponse(
    Guid Id,
    string FileName,
    string FileUrl,
    string ContentType,
    long FileSizeBytes,
    string MediaType,
    string ContainerName,
    Guid UploadedBy,
    string? UsageContext,
    Guid? EntityId,
    string? ThumbnailUrl,
    double? DurationSeconds,
    int? Width,
    int? Height,
    DateTime CreatedAt
);

/// <summary>
/// DTO for media list item
/// </summary>
public record MediaSummaryResponse(
    Guid Id,
    string FileName,
    string FileUrl,
    string ContentType,
    string MediaType,
    string? ThumbnailUrl,
    DateTime CreatedAt
);
