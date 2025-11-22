namespace GymHero.Domain.Entities;

/// <summary>
/// Tracks individual views of posts for analytics
/// </summary>
public class PostView : BaseEntity
{
    /// <summary>
    /// ID of the post that was viewed
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Navigation property to the post
    /// </summary>
    public Post Post { get; set; } = null!;

    /// <summary>
    /// ID of the user who viewed the post (null for anonymous views)
    /// </summary>
    public Guid? ViewerId { get; set; }

    /// <summary>
    /// Navigation property to the viewer
    /// </summary>
    public User? Viewer { get; set; }

    /// <summary>
    /// When the post was viewed
    /// </summary>
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Source/context of the view (e.g., "dashboard", "discover", "profile")
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Whether the viewer clicked on the author's profile
    /// </summary>
    public bool ClickedProfile { get; set; } = false;
}
