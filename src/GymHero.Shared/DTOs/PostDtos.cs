using System.ComponentModel.DataAnnotations;

namespace GymHero.Shared.DTOs;

/// <summary>
/// Request DTO for creating a new post
/// </summary>
public record CreatePostRequest(
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
    string Title,

    [Required(ErrorMessage = "O conteúdo é obrigatório")]
    string Content,

    string? ImageUrl,

    bool IsPublished = false
);

/// <summary>
/// Request DTO for updating an existing post
/// </summary>
public record UpdatePostRequest(
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
    string Title,

    [Required(ErrorMessage = "O conteúdo é obrigatório")]
    string Content,

    string? ImageUrl,

    bool IsPublished
);

/// <summary>
/// Response DTO for post details
/// </summary>
public record PostResponse(
    Guid Id,
    string Title,
    string Content,
    string? ImageUrl,
    Guid AuthorId,
    string AuthorName,
    string? AuthorProfilePictureUrl,
    string? AuthorProfileSlug,
    bool IsPublished,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    // Analytics
    int ViewCount,
    int UniqueViewers,
    int ProfileClicks,
    double EngagementRate
);

/// <summary>
/// Response DTO for post summary (used in lists/feeds)
/// </summary>
public record PostSummaryResponse(
    Guid Id,
    string Title,
    string ContentPreview, // First 200 characters
    string? ImageUrl,
    Guid AuthorId,
    string AuthorName,
    string? AuthorProfilePictureUrl,
    string? AuthorProfileSlug,
    bool IsPublished,
    DateTime? PublishedAt,
    DateTime CreatedAt
);
