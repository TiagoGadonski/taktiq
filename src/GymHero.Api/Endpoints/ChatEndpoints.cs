using System.Security.Claims;
using System.Text.RegularExpressions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymHero.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GymHero.Api.Endpoints;

public static class ChatEndpoints
{
    // Whitelist of allowed message types
    private static readonly HashSet<string> AllowedMessageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Text", "Image", "File"
    };

    // Basic HTML tag removal for XSS prevention
    private static string SanitizeContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        // Remove HTML tags and script content
        var sanitized = Regex.Replace(content, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"<[^>]+>", "");

        // Decode HTML entities to prevent stored XSS
        sanitized = System.Net.WebUtility.HtmlDecode(sanitized);

        return sanitized.Trim();
    }

    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chat")
            .WithTags("Chat")
            .RequireAuthorization();

        // Get all conversations for the current user
        group.MapGet("/conversations", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var conversations = await context.Conversations
                .Where(c => c.Participant1Id == userId || c.Participant2Id == userId)
                .Where(c => !c.IsArchived)
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new
                {
                    Conversation = c,
                    OtherUser = c.Participant1Id == userId ? c.Participant2 : c.Participant1,
                    UnreadCount = c.Messages.Count(m => m.SenderId != userId && m.ReadAt == null)
                })
                .ToListAsync(ct);

            var result = conversations.Select(c => new ConversationResponse(
                c.Conversation.Id,
                c.OtherUser.Id,
                c.OtherUser.Name,
                c.OtherUser.ProfilePictureUrl,
                c.OtherUser.Role,
                c.Conversation.LastMessageAt,
                c.Conversation.LastMessagePreview,
                c.Conversation.LastMessageSenderId,
                c.UnreadCount,
                c.Conversation.IsArchived
            )).ToList();

            return Results.Ok(result);
        })
        .WithName("GetConversations")
        .WithSummary("Get all conversations for the current user");

        // Get or create a conversation with another user
        group.MapGet("/conversations/with/{otherUserId:guid}", async (
            Guid otherUserId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            ILogger<ChatHub> logger,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Security: Prevent self-conversation
            if (userId == otherUserId)
            {
                return Results.BadRequest(new { message = "Não é possível criar conversa consigo mesmo" });
            }

            try
            {
                // Security: Verify the other user exists
                var otherUserExists = await context.Users.AnyAsync(u => u.Id == otherUserId, ct);
                if (!otherUserExists)
                {
                    return Results.NotFound(new { message = "Usuário não encontrado" });
                }

                // Check if conversation already exists (in either direction)
                var conversation = await context.Conversations
                    .Where(c =>
                        (c.Participant1Id == userId && c.Participant2Id == otherUserId) ||
                        (c.Participant1Id == otherUserId && c.Participant2Id == userId))
                    .Include(c => c.Participant1)
                    .Include(c => c.Participant2)
                    .Include(c => c.Messages.OrderBy(m => m.SentAt))
                    .FirstOrDefaultAsync(ct);

                // Create new conversation if it doesn't exist
                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        Id = Guid.NewGuid(),
                        Participant1Id = userId,
                        Participant2Id = otherUserId,
                        LastMessageAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Conversations.Add(conversation);
                    await context.SaveChangesAsync(ct);

                    // Reload with related data
                    conversation = await context.Conversations
                        .Include(c => c.Participant1)
                        .Include(c => c.Participant2)
                        .Include(c => c.Messages)
                        .FirstAsync(c => c.Id == conversation.Id, ct);
                }

                var otherUser = conversation.Participant1Id == userId
                    ? conversation.Participant2
                    : conversation.Participant1;

                var messages = conversation.Messages.Select(m => new MessageResponse(
                    m.Id,
                    m.ConversationId,
                    m.SenderId,
                    m.Sender.Name,
                    m.Sender.ProfilePictureUrl,
                    m.IsDeleted ? "[Mensagem deletada]" : m.Content,
                    m.MessageType,
                    m.FileUrl,
                    m.FileName,
                    m.SentAt,
                    m.ReadAt,
                    m.IsEdited,
                    m.EditedAt,
                    m.IsDeleted
                )).ToList();

                var response = new ConversationDetailsResponse(
                    conversation.Id,
                    otherUser.Id,
                    otherUser.Name,
                    otherUser.ProfilePictureUrl,
                    otherUser.Role,
                    messages,
                    messages.Count,
                    conversation.IsArchived
                );

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting/creating conversation with user {OtherUserId}", otherUserId);
                return Results.Problem("Erro ao carregar conversa");
            }
        })
        .WithName("GetOrCreateConversation")
        .WithSummary("Get or create a conversation with another user");

        // Send a message
        group.MapPost("/messages", async (
            [FromBody] SendMessageRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            IHubContext<ChatHub> hubContext,
            ILogger<ChatHub> logger,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userName = user.FindFirstValue(ClaimTypes.Name)!;
            var userProfilePic = user.FindFirstValue("ProfilePictureUrl");

            // Security: Validate message type
            if (!AllowedMessageTypes.Contains(request.MessageType))
            {
                logger.LogWarning("Invalid message type attempted: {MessageType} by user {UserId}", request.MessageType, userId);
                return Results.BadRequest(new { message = "Tipo de mensagem inválido" });
            }

            // Security: Sanitize content to prevent XSS
            var sanitizedContent = SanitizeContent(request.Content);
            if (string.IsNullOrWhiteSpace(sanitizedContent))
            {
                return Results.BadRequest(new { message = "Conteúdo da mensagem inválido" });
            }

            // Security: Verify recipient exists and prevent self-messaging
            if (userId == request.RecipientId)
            {
                return Results.BadRequest(new { message = "Não é possível enviar mensagens para si mesmo" });
            }

            var recipientExists = await context.Users.AnyAsync(u => u.Id == request.RecipientId, ct);
            if (!recipientExists)
            {
                return Results.NotFound(new { message = "Destinatário não encontrado" });
            }

            try
            {
                // Get or create conversation
                var conversation = await context.Conversations
                    .Where(c =>
                        (c.Participant1Id == userId && c.Participant2Id == request.RecipientId) ||
                        (c.Participant1Id == request.RecipientId && c.Participant2Id == userId))
                    .FirstOrDefaultAsync(ct);

                if (conversation == null)
                {
                    conversation = new Conversation
                    {
                        Id = Guid.NewGuid(),
                        Participant1Id = userId,
                        Participant2Id = request.RecipientId,
                        LastMessageAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Conversations.Add(conversation);
                }

                // Create the message with sanitized content
                var message = new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    SenderId = userId,
                    Content = sanitizedContent,
                    MessageType = request.MessageType,
                    FileUrl = request.FileUrl,
                    FileName = request.FileName,
                    SentAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                context.Messages.Add(message);

                // Update conversation
                conversation.LastMessageAt = message.SentAt;
                conversation.LastMessagePreview = sanitizedContent.Length > 100
                    ? sanitizedContent[..100] + "..."
                    : sanitizedContent;
                conversation.LastMessageSenderId = userId;

                await context.SaveChangesAsync(ct);

                // Send real-time notification via SignalR
                var notification = new MessageNotification(
                    message.Id,
                    conversation.Id,
                    userId,
                    userName,
                    userProfilePic,
                    sanitizedContent,
                    request.MessageType,
                    message.SentAt
                );

                await hubContext.Clients.Group($"user_{request.RecipientId}")
                    .SendAsync("ReceiveMessage", notification, ct);

                // Return the created message
                var response = new MessageResponse(
                    message.Id,
                    message.ConversationId,
                    userId,
                    userName,
                    userProfilePic,
                    message.Content,
                    message.MessageType,
                    message.FileUrl,
                    message.FileName,
                    message.SentAt,
                    null,
                    false,
                    null,
                    false
                );

                return Results.Created($"/api/chat/messages/{message.Id}", response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending message from user {UserId}", userId);
                return Results.Problem("Erro ao enviar mensagem");
            }
        })
        .WithName("SendMessage")
        .WithSummary("Send a message to another user");

        // Mark messages in a conversation as read
        group.MapPost("/conversations/{conversationId:guid}/mark-read", async (
            Guid conversationId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            IHubContext<ChatHub> hubContext,
            ILogger<ChatHub> logger,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                // Security: Verify user is a participant in this conversation
                var isParticipant = await context.Conversations
                    .AnyAsync(c => c.Id == conversationId &&
                                  (c.Participant1Id == userId || c.Participant2Id == userId), ct);

                if (!isParticipant)
                {
                    logger.LogWarning("User {UserId} attempted to mark messages as read in unauthorized conversation {ConversationId}",
                        userId, conversationId);
                    return Results.Forbid();
                }

                // Get all unread messages in this conversation that were sent to the current user
                var unreadMessages = await context.Messages
                    .Where(m => m.ConversationId == conversationId &&
                               m.SenderId != userId &&
                               m.ReadAt == null)
                    .ToListAsync(ct);

                var readAt = DateTime.UtcNow;
                foreach (var message in unreadMessages)
                {
                    message.ReadAt = readAt;

                    // Notify via SignalR
                    await hubContext.Clients.Group($"conversation_{conversationId}")
                        .SendAsync("MessageRead", new
                        {
                            MessageId = message.Id,
                            ReadByUserId = userId,
                            ReadAt = readAt
                        }, ct);
                }

                if (unreadMessages.Any())
                {
                    await context.SaveChangesAsync(ct);
                }

                return Results.Ok(new { markedAsRead = unreadMessages.Count });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error marking messages as read in conversation {ConversationId}", conversationId);
                return Results.Problem("Erro ao marcar mensagens como lidas");
            }
        })
        .WithName("MarkMessagesAsRead")
        .WithSummary("Mark all unread messages in a conversation as read");

        // Get unread message count
        group.MapGet("/unread-count", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var unreadByConversation = await context.Messages
                .Where(m => m.Conversation.Participant1Id == userId || m.Conversation.Participant2Id == userId)
                .Where(m => m.SenderId != userId && m.ReadAt == null)
                .GroupBy(m => m.ConversationId)
                .Select(g => new { ConversationId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ConversationId, x => x.Count, ct);

            var totalUnread = unreadByConversation.Values.Sum();

            var response = new UnreadMessagesCountResponse(
                totalUnread,
                unreadByConversation
            );

            return Results.Ok(response);
        })
        .WithName("GetUnreadMessageCount")
        .WithSummary("Get the count of unread messages");

        // Delete a message
        group.MapDelete("/messages/{messageId:guid}", async (
            Guid messageId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var message = await context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == userId, ct);

            if (message == null)
                return Results.NotFound(new { message = "Mensagem não encontrada" });

            message.IsDeleted = true;
            message.Content = "[Mensagem deletada]";
            await context.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteMessage")
        .WithSummary("Delete a message (soft delete)");

        // Edit a message
        group.MapPut("/messages/{messageId:guid}", async (
            Guid messageId,
            [FromBody] EditMessageRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            ILogger<ChatHub> logger,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                // Security: Sanitize content
                var sanitizedContent = SanitizeContent(request.Content);
                if (string.IsNullOrWhiteSpace(sanitizedContent))
                {
                    return Results.BadRequest(new { message = "Conteúdo da mensagem inválido" });
                }

                var message = await context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.SenderId == userId && !m.IsDeleted, ct);

                if (message == null)
                {
                    return Results.NotFound(new { message = "Mensagem não encontrada ou sem permissão para editar" });
                }

                message.Content = sanitizedContent;
                message.IsEdited = true;
                message.EditedAt = DateTime.UtcNow;

                await context.SaveChangesAsync(ct);

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error editing message {MessageId} by user {UserId}", messageId, userId);
                return Results.Problem("Erro ao editar mensagem");
            }
        })
        .WithName("EditMessage")
        .WithSummary("Edit a message");

        // Archive a conversation
        group.MapPost("/conversations/{conversationId:guid}/archive", async (
            Guid conversationId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var conversation = await context.Conversations
                .Where(c => c.Id == conversationId &&
                           (c.Participant1Id == userId || c.Participant2Id == userId))
                .FirstOrDefaultAsync(ct);

            if (conversation == null)
                return Results.NotFound(new { message = "Conversa não encontrada" });

            conversation.IsArchived = true;
            await context.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("ArchiveConversation")
        .WithSummary("Archive a conversation");
    }
}
