using System.ComponentModel.DataAnnotations;

namespace GymHero.Shared.DTOs;

/// <summary>
/// Request DTO for sending a message
/// </summary>
public record SendMessageRequest(
    [Required(ErrorMessage = "O destinatário é obrigatório")]
    Guid RecipientId,

    [Required(ErrorMessage = "O conteúdo da mensagem é obrigatório")]
    [StringLength(5000, ErrorMessage = "A mensagem deve ter no máximo 5000 caracteres")]
    string Content,

    string MessageType = "Text",
    string? FileUrl = null,
    string? FileName = null
);

/// <summary>
/// Response DTO for a message
/// </summary>
public record MessageResponse(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string SenderName,
    string? SenderProfilePictureUrl,
    string Content,
    string MessageType,
    string? FileUrl,
    string? FileName,
    DateTime SentAt,
    DateTime? ReadAt,
    bool IsEdited,
    DateTime? EditedAt,
    bool IsDeleted
);

/// <summary>
/// Response DTO for a conversation
/// </summary>
public record ConversationResponse(
    Guid Id,
    Guid OtherUserId,
    string OtherUserName,
    string? OtherUserProfilePictureUrl,
    string? OtherUserRole,
    DateTime LastMessageAt,
    string? LastMessagePreview,
    Guid? LastMessageSenderId,
    int UnreadCount,
    bool IsArchived
);

/// <summary>
/// Response DTO for conversation details with messages
/// </summary>
public record ConversationDetailsResponse(
    Guid Id,
    Guid OtherUserId,
    string OtherUserName,
    string? OtherUserProfilePictureUrl,
    string? OtherUserRole,
    List<MessageResponse> Messages,
    int TotalMessages,
    bool IsArchived
);

/// <summary>
/// Request DTO for marking messages as read
/// </summary>
public record MarkMessagesAsReadRequest(
    [Required(ErrorMessage = "O ID da conversa é obrigatório")]
    Guid ConversationId
);

/// <summary>
/// Request DTO for editing a message
/// </summary>
public record EditMessageRequest(
    [Required(ErrorMessage = "O conteúdo é obrigatório")]
    [StringLength(5000, ErrorMessage = "A mensagem deve ter no máximo 5000 caracteres")]
    string Content
);

/// <summary>
/// Response DTO for unread message count
/// </summary>
public record UnreadMessagesCountResponse(
    int TotalUnreadCount,
    Dictionary<Guid, int> UnreadCountByConversation
);

/// <summary>
/// Real-time message notification DTO (sent via SignalR)
/// </summary>
public record MessageNotification(
    Guid MessageId,
    Guid ConversationId,
    Guid SenderId,
    string SenderName,
    string? SenderProfilePictureUrl,
    string Content,
    string MessageType,
    DateTime SentAt
);

/// <summary>
/// Typing indicator DTO (sent via SignalR)
/// </summary>
public record TypingIndicator(
    Guid ConversationId,
    Guid UserId,
    string UserName,
    bool IsTyping
);
