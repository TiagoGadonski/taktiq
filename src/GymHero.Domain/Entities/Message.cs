namespace GymHero.Domain.Entities;

/// <summary>
/// Represents a single message in a conversation
/// </summary>
public class Message : BaseEntity
{
    /// <summary>
    /// ID of the conversation this message belongs to
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Navigation property to the conversation
    /// </summary>
    public Conversation Conversation { get; set; } = null!;

    /// <summary>
    /// ID of the user who sent this message
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// Navigation property to the sender
    /// </summary>
    public User Sender { get; set; } = null!;

    /// <summary>
    /// The message content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Type of message: Text, Image, File, Video
    /// </summary>
    public string MessageType { get; set; } = "Text";

    /// <summary>
    /// File URL if the message contains a file/image/video
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// File name if the message contains a file
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// When the message was sent
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the message was read by the recipient (null if unread)
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Whether the message has been edited
    /// </summary>
    public bool IsEdited { get; set; } = false;

    /// <summary>
    /// When the message was last edited
    /// </summary>
    public DateTime? EditedAt { get; set; }

    /// <summary>
    /// Whether the message has been deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
