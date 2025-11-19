using System.Security.Claims;
using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using GymHero.Application.Features.Sessions.Commands;
using GymHero.Application.Features.Sessions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public record UpdateSetRequest(int? Reps, double? Weight, int? Rpe);

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sessions")
                       .WithTags("Workout Sessions")
                       .RequireAuthorization();

        group.MapGet("/current", async (
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetCurrentSessionQuery(ownerId);
            var session = await sender.Send(query);
            return Results.Ok(session);
        })
        .WithName("GetCurrentSession")
        .WithSummary("Gets the current active workout session for the user");

        group.MapGet("", async (
            ClaimsPrincipal user,
            ISender sender,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetSessionHistoryQuery(userId, page, pageSize, startDate, endDate);
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetSessionHistory")
        .WithSummary("Gets paginated workout session history for the user");

        group.MapPost("/start", async (
            [FromBody] StartSessionRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new StartWorkoutSessionCommand(request.WorkoutPlanId, request.WorkoutId, ownerId);

            try
            {
                var sessionId = await sender.Send(command);
                return Results.Created($"/api/sessions/{sessionId}", new { sessionId });
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("StartWorkoutSession")
        .WithSummary("Starts a new workout session based on a workout plan");

        group.MapPost("/{sessionId:guid}/sets", async (
            Guid sessionId,
            [FromBody] LogSetRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new LogSetCommand(
                sessionId,
                ownerId,
                request.ExerciseId,
                request.SetNumber,
                request.Reps,
                request.Load,
                request.Rpe,
                request.IsAddedDuringSession
            );

            try
            {
                var response = await sender.Send(command);
                return Results.Created($"/api/v1/sessions/{sessionId}/sets/{response.SetId}", response);
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("LogWorkoutSet")
        .WithSummary("Logs a single set of an exercise within an active workout session");

        group.MapPatch("/{sessionId:guid}/complete", async (
            Guid sessionId,
            [FromBody] CompleteSessionRequest? request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new CompleteWorkoutSessionCommand(sessionId, ownerId, request?.Notes);

            try
            {
                var result = await sender.Send(command);
                return Results.Ok(result);
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("CompleteWorkoutSession")
        .WithSummary("Marks a workout session as complete with optional notes");

        group.MapPatch("/{sessionId:guid}/cancel", async (
            Guid sessionId,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new CancelWorkoutSessionCommand(sessionId, ownerId);

            try
            {
                await sender.Send(command);
                return Results.NoContent();
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("CancelWorkoutSession")
        .WithSummary("Cancels an active workout session and removes all progress");
    }

    public static void MapSetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sets")
                       .WithTags("Workout Sets")
                       .RequireAuthorization();

        group.MapPatch("/{setId:guid}", async (
            Guid setId,
            [FromBody] UpdateSetRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var set = await context.WorkoutSets
                .Include(s => s.WorkoutSession)
                .FirstOrDefaultAsync(s => s.Id == setId, cancellationToken);

            if (set == null)
                return Results.NotFound(new { message = "Set not found" });

            if (set.WorkoutSession.OwnerId != ownerId)
                return Results.Forbid();

            if (request.Reps.HasValue)
                set.Reps = request.Reps;
            if (request.Weight.HasValue)
                set.Load = request.Weight;
            if (request.Rpe.HasValue)
                set.Rpe = request.Rpe;

            await context.SaveChangesAsync(cancellationToken);

            return Results.Ok(set);
        })
        .WithName("UpdateWorkoutSet")
        .WithSummary("Updates an existing workout set");

        group.MapDelete("/{setId:guid}", async (
            Guid setId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var set = await context.WorkoutSets
                .Include(s => s.WorkoutSession)
                .FirstOrDefaultAsync(s => s.Id == setId, cancellationToken);

            if (set == null)
                return Results.NotFound(new { message = "Set not found" });

            if (set.WorkoutSession.OwnerId != ownerId)
                return Results.Forbid();

            context.WorkoutSets.Remove(set);
            await context.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .WithName("DeleteWorkoutSet")
        .WithSummary("Deletes a workout set");
    }
}