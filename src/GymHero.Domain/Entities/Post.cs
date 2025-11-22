namespace GymHero.Domain.Entities;

/// <summary>
/// Represents a blog post created by a Personal Trainer
/// </summary>
public class Post : BaseEntity
{
    /// <summary>
    /// Post title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Post content (markdown supported)
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Optional cover image URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// ID of the Personal Trainer who created this post
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Navigation property to the author (Personal Trainer)
    /// </summary>
    public User Author { get; set; } = null!;

    /// <summary>
    /// Whether this post is published and visible to students
    /// </summary>
    public bool IsPublished { get; set; } = false;

    /// <summary>
    /// When this post was published (null if draft)
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Last time this post was updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total number of views (cached count for performance)
    /// </summary>
    public int ViewCount { get; set; } = 0;

    /// <summary>
    /// Navigation property to all views of this post
    /// </summary>
    public ICollection<PostView> Views { get; set; } = new List<PostView>();
}
