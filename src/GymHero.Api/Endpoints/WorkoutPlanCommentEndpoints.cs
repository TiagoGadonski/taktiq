using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class WorkoutPlanCommentEndpoints
{
    public static void MapWorkoutPlanCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workout-plans/{planId:guid}/comments")
            .WithTags("Workout Plan Comments")
            .RequireAuthorization();

        // Create a comment on a workout plan
        group.MapPost("", async (
            Guid planId,
            CreateWorkoutPlanCommentRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Verify the workout plan exists
            var planExists = await context.WorkoutPlans
                .AnyAsync(p => p.Id == planId, cancellationToken);

            if (!planExists)
            {
                return Results.NotFound(new { message = "Workout plan not found" });
            }

            // If it's a reply, verify parent comment exists
            if (request.ParentCommentId.HasValue)
            {
                var parentExists = await context.WorkoutPlanComments
                    .AnyAsync(c => c.Id == request.ParentCommentId.Value && c.WorkoutPlanId == planId,
                        cancellationToken);

                if (!parentExists)
                {
                    return Results.NotFound(new { message = "Parent comment not found" });
                }
            }

            var comment = new WorkoutPlanComment
            {
                WorkoutPlanId = planId,
                UserId = userId,
                Content = request.Content,
                ParentCommentId = request.ParentCommentId
            };

            context.WorkoutPlanComments.Add(comment);
            await context.SaveChangesAsync(cancellationToken);

            // Load user info for response
            var userInfo = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.Name })
                .FirstOrDefaultAsync(cancellationToken);

            var response = new WorkoutPlanCommentResponse(
                comment.Id,
                comment.WorkoutPlanId,
                comment.UserId,
                userInfo?.Name ?? "Unknown",
                comment.Content,
                comment.ParentCommentId,
                comment.CreatedAt,
                new List<WorkoutPlanCommentResponse>()
            );

            return Results.Ok(response);
        })
        .WithName("CreateWorkoutPlanComment")
        .WithSummary("Create a comment on a workout plan");

        // Get all comments for a workout plan
        group.MapGet("", async (
            Guid planId,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var comments = await context.WorkoutPlanComments
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.WorkoutPlanId == planId && !c.IsDeleted && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);

            var response = comments.Select(c => MapToResponse(c)).ToList();

            return Results.Ok(response);
        })
        .WithName("GetWorkoutPlanComments")
        .WithSummary("Get all comments for a workout plan");

        // Update a comment
        group.MapPut("/{commentId:guid}", async (
            Guid planId,
            Guid commentId,
            UpdateWorkoutPlanCommentRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var comment = await context.WorkoutPlanComments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.WorkoutPlanId == planId,
                    cancellationToken);

            if (comment == null)
            {
                return Results.NotFound(new { message = "Comment not found" });
            }

            // Only the author can edit their comment
            if (comment.UserId != userId)
            {
                return Results.Forbid();
            }

            comment.Content = request.Content;
            await context.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .WithName("UpdateWorkoutPlanComment")
        .WithSummary("Update a comment");

        // Delete a comment
        group.MapDelete("/{commentId:guid}", async (
            Guid planId,
            Guid commentId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var comment = await context.WorkoutPlanComments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.WorkoutPlanId == planId,
                    cancellationToken);

            if (comment == null)
            {
                return Results.NotFound(new { message = "Comment not found" });
            }

            // Only the author can delete their comment
            if (comment.UserId != userId)
            {
                return Results.Forbid();
            }

            // Soft delete
            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .WithName("DeleteWorkoutPlanComment")
        .WithSummary("Delete a comment");
    }

    private static WorkoutPlanCommentResponse MapToResponse(WorkoutPlanComment comment)
    {
        var replies = comment.Replies
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.CreatedAt)
            .Select(r => MapToResponse(r))
            .ToList();

        return new WorkoutPlanCommentResponse(
            comment.Id,
            comment.WorkoutPlanId,
            comment.UserId,
            comment.User?.Name ?? "Unknown",
            comment.Content,
            comment.ParentCommentId,
            comment.CreatedAt,
            replies
        );
    }
}
