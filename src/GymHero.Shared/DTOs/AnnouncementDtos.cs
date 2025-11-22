using System.ComponentModel.DataAnnotations;

namespace GymHero.Shared.DTOs;

/// <summary>
/// Request DTO for creating an announcement
/// </summary>
public record CreateAnnouncementRequest(
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
    string Title,

    [Required(ErrorMessage = "O conteúdo é obrigatório")]
    string Content,

    [Required(ErrorMessage = "O tipo é obrigatório")]
    string Type, // NewFeature, Maintenance, Tips, General

    string? ImageUrl,

    DateTime? ExpiresAt,

    [Range(1, 5, ErrorMessage = "A prioridade deve ser entre 1 e 5")]
    int Priority = 3,

    bool ShowAsPopup = true,
    bool IsActive = true
);

/// <summary>
/// Request DTO for updating an announcement
/// </summary>
public record UpdateAnnouncementRequest(
    [Required(ErrorMessage = "O título é obrigatório")]
    [StringLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres")]
    string Title,

    [Required(ErrorMessage = "O conteúdo é obrigatório")]
    string Content,

    [Required(ErrorMessage = "O tipo é obrigatório")]
    string Type,

    string? ImageUrl,

    DateTime? ExpiresAt,

    [Range(1, 5, ErrorMessage = "A prioridade deve ser entre 1 e 5")]
    int Priority,

    bool ShowAsPopup,
    bool IsActive
);

/// <summary>
/// Response DTO for announcement
/// </summary>
public record AnnouncementResponse(
    Guid Id,
    string Title,
    string Content,
    string Type,
    string? ImageUrl,
    DateTime PublishedAt,
    DateTime? ExpiresAt,
    int Priority,
    bool ShowAsPopup,
    bool IsActive,
    bool IsCurrentlyActive,
    int ReadCount,
    DateTime CreatedAt
);

/// <summary>
/// Response DTO for announcement with user read status
/// </summary>
public record AnnouncementWithReadStatusResponse(
    Guid Id,
    string Title,
    string Content,
    string Type,
    string? ImageUrl,
    DateTime PublishedAt,
    DateTime? ExpiresAt,
    int Priority,
    bool ShowAsPopup,
    bool IsRead,
    DateTime? ReadAt
);
