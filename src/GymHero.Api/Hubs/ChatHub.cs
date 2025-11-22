using System.Security.Claims;
using GymHero.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GymHero.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time chat functionality
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal group for targeted messages
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} connected to chat with connection {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} disconnected from chat", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Send a typing indicator to another user
    /// </summary>
    /// <param name="conversationId">ID of the conversation</param>
    /// <param name="recipientId">ID of the user to notify</param>
    /// <param name="isTyping">Whether the user is typing</param>
    public async Task SendTypingIndicator(Guid conversationId, Guid recipientId, bool isTyping)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = Context.User?.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
            return;

        var typingIndicator = new TypingIndicator(
            conversationId,
            Guid.Parse(userId),
            userName,
            isTyping
        );

        // Send to the recipient's group
        await Clients.Group($"user_{recipientId}").SendAsync("UserTyping", typingIndicator);
    }

    /// <summary>
    /// Join a conversation room (for real-time updates in that conversation)
    /// </summary>
    /// <param name="conversationId">ID of the conversation to join</param>
    public async Task JoinConversation(Guid conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        _logger.LogInformation("User joined conversation {ConversationId}", conversationId);
    }

    /// <summary>
    /// Leave a conversation room
    /// </summary>
    /// <param name="conversationId">ID of the conversation to leave</param>
    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        _logger.LogInformation("User left conversation {ConversationId}", conversationId);
    }

    /// <summary>
    /// Notify a specific user about a new message
    /// </summary>
    /// <param name="recipientId">ID of the recipient user</param>
    /// <param name="message">The message notification</param>
    public async Task NotifyNewMessage(Guid recipientId, MessageNotification message)
    {
        await Clients.Group($"user_{recipientId}").SendAsync("ReceiveMessage", message);
    }

    /// <summary>
    /// Notify a conversation that a message has been read
    /// </summary>
    /// <param name="conversationId">ID of the conversation</param>
    /// <param name="messageId">ID of the message that was read</param>
    /// <param name="readByUserId">ID of the user who read the message</param>
    public async Task NotifyMessageRead(Guid conversationId, Guid messageId, Guid readByUserId)
    {
        await Clients.Group($"conversation_{conversationId}").SendAsync("MessageRead", new
        {
            MessageId = messageId,
            ReadByUserId = readByUserId,
            ReadAt = DateTime.UtcNow
        });
    }
}
