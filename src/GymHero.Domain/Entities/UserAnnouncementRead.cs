namespace GymHero.Domain.Entities;

/// <summary>
/// Tracks which announcements each user has read/dismissed
/// </summary>
public class UserAnnouncementRead : BaseEntity
{
    /// <summary>
    /// ID of the user who read the announcement
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// ID of the announcement that was read
    /// </summary>
    public Guid AnnouncementId { get; set; }

    /// <summary>
    /// Navigation property to the announcement
    /// </summary>
    public Announcement Announcement { get; set; } = null!;

    /// <summary>
    /// When the user read/dismissed the announcement
    /// </summary>
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}
