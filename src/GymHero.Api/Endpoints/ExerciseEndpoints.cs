using GymHero.Application.Common.Exceptions;
using GymHero.Shared.DTOs;
using GymHero.Application.Features.Exercises.Commands;
using GymHero.Application.Features.Exercises.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymHero.Api.Endpoints;

public static class ExerciseEndpoints
{
    public static void MapExerciseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/exercises").WithTags("Exercises");

        // GET /api/v1/exercises
        group.MapGet("/", async (int? workoutLocation, ISender sender) =>
        {
            var result = await sender.Send(new GetAllExercisesQuery(workoutLocation));
            return Results.Ok(result);
        });

        // GET /api/v1/exercises/{id}
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetExerciseByIdQuery(id);
            var result = await sender.Send(query);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        // POST /api/v1/exercises
        group.MapPost("/", async ([FromBody] CreateExerciseRequest request, ISender sender) =>
        {
            var workoutLocation = (GymHero.Domain.Enums.WorkoutLocation)request.WorkoutLocation;
            var command = new CreateExerciseCommand(request.Name, request.MuscleGroup, request.Category, request.Equipment, request.Notes, request.VideoUrl, request.ImageUrl, workoutLocation);
            var result = await sender.Send(command);
            return Results.Created($"/api/exercises/{result.Id}", result);
        }).RequireAuthorization(); // Allow authenticated users to create exercises

        // PUT /api/v1/exercises/{id}
        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateExerciseRequest request, ISender sender) =>
        {
            var command = new UpdateExerciseCommand(id, request.Name, request.MuscleGroup, request.Category, request.Equipment, request.Notes, request.VideoUrl, request.ImageUrl);
            try
            {
                await sender.Send(command);
                return Results.NoContent();
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        }).RequireAuthorization(); // Allow any authenticated user to update exercises

        // DELETE /api/v1/exercises/{id}
        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteExerciseCommand(id);
            try
            {
                await sender.Send(command);
                return Results.NoContent();
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        }).RequireAuthorization("AdminOrPersonalPolicy");
    }
}