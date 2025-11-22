using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Hubs;

/// <summary>
/// SignalR Hub for real-time chat functionality
/// Implements secure real-time messaging with authorization checks
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly IApplicationDbContext _context;

    public ChatHub(ILogger<ChatHub> logger, IApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Called when a client connects
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            try
            {
                // Add user to their personal group for targeted messages
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogDebug("User {UserId} connected to chat", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during connection setup for user {UserId}", userId);
                throw;
            }
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
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
                _logger.LogDebug("User {UserId} disconnected from chat", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during disconnection cleanup for user {UserId}", userId);
            }
        }

        if (exception != null)
        {
            _logger.LogWarning(exception, "User {UserId} disconnected with error", userId);
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
        {
            _logger.LogWarning("Typing indicator attempted without valid user context");
            return;
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Security: Verify user is a participant in the conversation
            var isParticipant = await _context.Conversations
                .AnyAsync(c => c.Id == conversationId &&
                              (c.Participant1Id == userGuid || c.Participant2Id == userGuid));

            if (!isParticipant)
            {
                _logger.LogWarning("User {UserId} attempted to send typing indicator for unauthorized conversation {ConversationId}",
                    userId, conversationId);
                return;
            }

            var typingIndicator = new TypingIndicator(
                conversationId,
                userGuid,
                userName,
                isTyping
            );

            // Send to the recipient's group only
            await Clients.Group($"user_{recipientId}").SendAsync("UserTyping", typingIndicator);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending typing indicator for conversation {ConversationId}", conversationId);
        }
    }

    /// <summary>
    /// Join a conversation room (for real-time updates in that conversation)
    /// </summary>
    /// <param name="conversationId">ID of the conversation to join</param>
    public async Task JoinConversation(Guid conversationId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Join conversation attempted without valid user context");
            return;
        }

        try
        {
            var userGuid = Guid.Parse(userId);

            // Security: Verify user is a participant in the conversation before allowing join
            var isParticipant = await _context.Conversations
                .AnyAsync(c => c.Id == conversationId &&
                              (c.Participant1Id == userGuid || c.Participant2Id == userGuid));

            if (!isParticipant)
            {
                _logger.LogWarning("User {UserId} attempted to join unauthorized conversation {ConversationId}",
                    userId, conversationId);
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            _logger.LogDebug("User {UserId} joined conversation {ConversationId}", userId, conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining conversation {ConversationId}", conversationId);
        }
    }

    /// <summary>
    /// Leave a conversation room
    /// </summary>
    /// <param name="conversationId">ID of the conversation to leave</param>
    public async Task LeaveConversation(Guid conversationId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            _logger.LogDebug("User {UserId} left conversation {ConversationId}", userId, conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving conversation {ConversationId}", conversationId);
        }
    }
}
