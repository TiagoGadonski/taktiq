namespace GymHero.Domain.Entities;

/// <summary>
/// Represents an uploaded media file (image or video)
/// </summary>
public class Media : BaseEntity
{
    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// URL of the file in blob storage
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>
    /// Content type (e.g., image/jpeg, video/mp4)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Type of media (Image, Video)
    /// </summary>
    public MediaType MediaType { get; set; }

    /// <summary>
    /// Container/folder name in blob storage
    /// </summary>
    public string ContainerName { get; set; } = "media";

    /// <summary>
    /// ID of the user who uploaded this media
    /// </summary>
    public Guid UploadedBy { get; set; }

    /// <summary>
    /// Navigation property to the user who uploaded this media
    /// </summary>
    public User Uploader { get; set; } = null!;

    /// <summary>
    /// What this media is used for (e.g., ProfilePicture, PostImage, ExerciseVideo)
    /// </summary>
    public string? UsageContext { get; set; }

    /// <summary>
    /// Optional reference to the entity this media belongs to (Post, Exercise, etc.)
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Optional thumbnail URL for videos
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Video duration in seconds (for videos only)
    /// </summary>
    public double? DurationSeconds { get; set; }

    /// <summary>
    /// Video width in pixels (for videos only)
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Video height in pixels (for videos only)
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Whether this media file has been soft-deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// When this media was marked as deleted
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}

/// <summary>
/// Enum for media types
/// </summary>
public enum MediaType
{
    Image = 0,
    Video = 1
}
