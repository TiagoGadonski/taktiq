using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class AnnouncementEndpoints
{
    public static void MapAnnouncementEndpoints(this IEndpointRouteBuilder app)
    {
        // Admin endpoints - require admin authorization
        var adminGroup = app.MapGroup("/api/admin/announcements")
            .WithTags("Admin - Announcements")
            .RequireAuthorization("RequireAdminRole");

        // Create a new announcement
        adminGroup.MapPost("", async (
            [FromBody] CreateAnnouncementRequest request,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var announcement = new Announcement
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                Type = request.Type,
                ImageUrl = request.ImageUrl,
                PublishedAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt,
                Priority = request.Priority,
                ShowAsPopup = request.ShowAsPopup,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            context.Announcements.Add(announcement);
            await context.SaveChangesAsync(ct);

            return Results.Created($"/api/admin/announcements/{announcement.Id}", new { id = announcement.Id });
        })
        .WithName("CreateAnnouncement")
        .WithSummary("Creates a new platform announcement");

        // Get all announcements (admin view)
        adminGroup.MapGet("", async (
            IApplicationDbContext context,
            [FromQuery] bool? activeOnly,
            CancellationToken ct) =>
        {
            var query = context.Announcements.AsQueryable();

            if (activeOnly.HasValue && activeOnly.Value)
                query = query.Where(a => a.IsActive);

            var announcements = await query
                .OrderByDescending(a => a.PublishedAt)
                .Select(a => new AnnouncementResponse(
                    a.Id,
                    a.Title,
                    a.Content,
                    a.Type,
                    a.ImageUrl,
                    a.PublishedAt,
                    a.ExpiresAt,
                    a.Priority,
                    a.ShowAsPopup,
                    a.IsActive,
                    a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow),
                    a.Reads.Count,
                    a.CreatedAt
                ))
                .ToListAsync(ct);

            return Results.Ok(announcements);
        })
        .WithName("GetAllAnnouncements_Admin")
        .WithSummary("Gets all announcements for admin management");

        // Get announcement by ID (admin)
        adminGroup.MapGet("/{announcementId:guid}", async (
            Guid announcementId,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var announcement = await context.Announcements
                .Where(a => a.Id == announcementId)
                .Select(a => new AnnouncementResponse(
                    a.Id,
                    a.Title,
                    a.Content,
                    a.Type,
                    a.ImageUrl,
                    a.PublishedAt,
                    a.ExpiresAt,
                    a.Priority,
                    a.ShowAsPopup,
                    a.IsActive,
                    a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow),
                    a.Reads.Count,
                    a.CreatedAt
                ))
                .FirstOrDefaultAsync(ct);

            if (announcement == null)
                return Results.NotFound(new { message = "Anúncio não encontrado" });

            return Results.Ok(announcement);
        })
        .WithName("GetAnnouncementById_Admin")
        .WithSummary("Gets a specific announcement by ID");

        // Update an announcement
        adminGroup.MapPut("/{announcementId:guid}", async (
            Guid announcementId,
            [FromBody] UpdateAnnouncementRequest request,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var announcement = await context.Announcements
                .FirstOrDefaultAsync(a => a.Id == announcementId, ct);

            if (announcement == null)
                return Results.NotFound(new { message = "Anúncio não encontrado" });

            announcement.Title = request.Title;
            announcement.Content = request.Content;
            announcement.Type = request.Type;
            announcement.ImageUrl = request.ImageUrl;
            announcement.ExpiresAt = request.ExpiresAt;
            announcement.Priority = request.Priority;
            announcement.ShowAsPopup = request.ShowAsPopup;
            announcement.IsActive = request.IsActive;

            await context.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("UpdateAnnouncement")
        .WithSummary("Updates an existing announcement");

        // Delete an announcement
        adminGroup.MapDelete("/{announcementId:guid}", async (
            Guid announcementId,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var announcement = await context.Announcements
                .FirstOrDefaultAsync(a => a.Id == announcementId, ct);

            if (announcement == null)
                return Results.NotFound(new { message = "Anúncio não encontrado" });

            context.Announcements.Remove(announcement);
            await context.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteAnnouncement")
        .WithSummary("Deletes an announcement");

        // User endpoints - require authentication
        var userGroup = app.MapGroup("/api/announcements")
            .WithTags("Announcements - User")
            .RequireAuthorization();

        // Get all active announcements for user (with read status)
        userGroup.MapGet("", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            [FromQuery] bool? unreadOnly,
            [FromQuery] bool? popupOnly,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var query = context.Announcements
                .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow));

            if (popupOnly.HasValue && popupOnly.Value)
                query = query.Where(a => a.ShowAsPopup);

            var announcements = await query
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.PublishedAt)
                .Select(a => new
                {
                    Announcement = a,
                    Read = a.Reads.FirstOrDefault(r => r.UserId == userId)
                })
                .ToListAsync(ct);

            var result = announcements
                .Where(x => !unreadOnly.HasValue || !unreadOnly.Value || x.Read == null)
                .Select(x => new AnnouncementWithReadStatusResponse(
                    x.Announcement.Id,
                    x.Announcement.Title,
                    x.Announcement.Content,
                    x.Announcement.Type,
                    x.Announcement.ImageUrl,
                    x.Announcement.PublishedAt,
                    x.Announcement.ExpiresAt,
                    x.Announcement.Priority,
                    x.Announcement.ShowAsPopup,
                    x.Read != null,
                    x.Read?.ReadAt
                ))
                .ToList();

            return Results.Ok(result);
        })
        .WithName("GetUserAnnouncements")
        .WithSummary("Gets all active announcements with user read status");

        // Get unread popup count
        userGroup.MapGet("/unread-count", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var unreadCount = await context.Announcements
                .Where(a => a.IsActive &&
                           a.ShowAsPopup &&
                           (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow) &&
                           !a.Reads.Any(r => r.UserId == userId))
                .CountAsync(ct);

            return Results.Ok(new { unreadCount });
        })
        .WithName("GetUnreadAnnouncementCount")
        .WithSummary("Gets count of unread popup announcements for user");

        // Mark announcement as read
        userGroup.MapPost("/{announcementId:guid}/mark-read", async (
            Guid announcementId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Check if already marked as read
            var existingRead = await context.UserAnnouncementReads
                .AnyAsync(r => r.UserId == userId && r.AnnouncementId == announcementId, ct);

            if (existingRead)
                return Results.Ok(new { message = "Anúncio já marcado como lido" });

            // Check if announcement exists
            var announcementExists = await context.Announcements
                .AnyAsync(a => a.Id == announcementId, ct);

            if (!announcementExists)
                return Results.NotFound(new { message = "Anúncio não encontrado" });

            var read = new UserAnnouncementRead
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AnnouncementId = announcementId,
                ReadAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            context.UserAnnouncementReads.Add(read);
            await context.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Anúncio marcado como lido" });
        })
        .WithName("MarkAnnouncementAsRead")
        .WithSummary("Marks an announcement as read/dismissed by the user");

        // Mark all announcements as read
        userGroup.MapPost("/mark-all-read", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Get all active unread announcements
            var unreadAnnouncements = await context.Announcements
                .Where(a => a.IsActive &&
                           (a.ExpiresAt == null || a.ExpiresAt > DateTime.UtcNow) &&
                           !a.Reads.Any(r => r.UserId == userId))
                .Select(a => a.Id)
                .ToListAsync(ct);

            foreach (var announcementId in unreadAnnouncements)
            {
                var read = new UserAnnouncementRead
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AnnouncementId = announcementId,
                    ReadAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                context.UserAnnouncementReads.Add(read);
            }

            await context.SaveChangesAsync(ct);

            return Results.Ok(new { message = $"{unreadAnnouncements.Count} anúncios marcados como lidos" });
        })
        .WithName("MarkAllAnnouncementsAsRead")
        .WithSummary("Marks all announcements as read for the user");
    }
}
