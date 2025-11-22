namespace GymHero.Domain.Entities;

/// <summary>
/// Represents a platform announcement or update
/// </summary>
public class Announcement : BaseEntity
{
    /// <summary>
    /// Title of the announcement
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content/body of the announcement (supports markdown)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Type/category of announcement
    /// </summary>
    public string Type { get; set; } = "General"; // NewFeature, Maintenance, Tips, General, etc.

    /// <summary>
    /// Optional image URL for the announcement
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// When the announcement was published
    /// </summary>
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional expiration date (null = never expires)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Priority level (1-5, higher = more important)
    /// </summary>
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Whether to show as a popup (true) or just in the feed (false)
    /// </summary>
    public bool ShowAsPopup { get; set; } = true;

    /// <summary>
    /// Whether the announcement is active/published
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property to track which users have read this announcement
    /// </summary>
    public ICollection<UserAnnouncementRead> Reads { get; set; } = new List<UserAnnouncementRead>();

    /// <summary>
    /// Whether this announcement is currently active and not expired
    /// </summary>
    public bool IsCurrentlyActive
    {
        get
        {
            if (!IsActive) return false;
            if (ExpiresAt == null) return true;
            return ExpiresAt > DateTime.UtcNow;
        }
    }
}
