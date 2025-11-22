namespace GymHero.Domain.Entities;

/// <summary>
/// Represents a conversation between two users (typically trainer and student)
/// </summary>
public class Conversation : BaseEntity
{
    /// <summary>
    /// ID of the first participant (usually the trainer)
    /// </summary>
    public Guid Participant1Id { get; set; }

    /// <summary>
    /// Navigation property to first participant
    /// </summary>
    public User Participant1 { get; set; } = null!;

    /// <summary>
    /// ID of the second participant (usually the student)
    /// </summary>
    public Guid Participant2Id { get; set; }

    /// <summary>
    /// Navigation property to second participant
    /// </summary>
    public User Participant2 { get; set; } = null!;

    /// <summary>
    /// Timestamp of the last message in this conversation
    /// </summary>
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Preview of the last message (for conversation list)
    /// </summary>
    public string? LastMessagePreview { get; set; }

    /// <summary>
    /// ID of the user who sent the last message
    /// </summary>
    public Guid? LastMessageSenderId { get; set; }

    /// <summary>
    /// Whether this conversation is archived
    /// </summary>
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// All messages in this conversation
    /// </summary>
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
