using System.Security.Claims;
using GymHero.Application.Features.Notifications.Commands;
using GymHero.Application.Features.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymHero.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        // Get all notifications for the current user
        group.MapGet("/", async (
            ClaimsPrincipal user,
            [FromQuery] bool? unreadOnly,
            ISender sender) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetUserNotificationsQuery(userId, unreadOnly);
            var notifications = await sender.Send(query);
            return Results.Ok(notifications);
        })
        .WithName("GetNotifications")
        .WithSummary("Gets all notifications for the authenticated user");

        // Get unread notification count
        group.MapGet("/unread-count", async (
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetUserNotificationsQuery(userId, UnreadOnly: true);
            var notifications = await sender.Send(query);
            return Results.Ok(new { count = notifications.Count() });
        })
        .WithName("GetUnreadNotificationCount")
        .WithSummary("Gets the count of unread notifications");

        // Mark a notification as read
        group.MapPatch("/{id:guid}/read", async (
            Guid id,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new MarkNotificationAsReadCommand(id, userId);
            var result = await sender.Send(command);

            return result
                ? Results.NoContent()
                : Results.NotFound(new { message = "Notificação não encontrada" });
        })
        .WithName("MarkNotificationAsRead")
        .WithSummary("Marks a specific notification as read");

        // Mark all notifications as read
        group.MapPatch("/read-all", async (
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new MarkAllNotificationsAsReadCommand(userId);
            var count = await sender.Send(command);

            return Results.Ok(new { message = $"{count} notificação(ões) marcada(s) como lida(s)", count });
        })
        .WithName("MarkAllNotificationsAsRead")
        .WithSummary("Marks all notifications as read for the authenticated user");
    }
}
