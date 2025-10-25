using System.Security.Claims;
using GymHero.Shared.DTOs;
using GymHero.Application.Features.WorkoutPlans.Commands;
using GymHero.Application.Features.WorkoutPlans.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using GymHero.Application.Common.Interfaces; // <<<--- LINHA ADICIONADA
using Microsoft.EntityFrameworkCore;
using GymHero.Application.Common.Exceptions;         // <<<--- ADICIONE ESTA TAMBÉM

namespace GymHero.Api.Endpoints;

public static class WorkoutPlanEndpoints
{
    public static void MapWorkoutPlanEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workout-plans")
                       .WithTags("Workout Plans")
                       .RequireAuthorization();

        // Endpoint para CRIAR (já existente)
        group.MapPost("/", async (
            [FromBody] CreateWorkoutPlanRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new CreateWorkoutPlanCommand(request.Name, request.Goal, ownerId);
            var result = await sender.Send(command);
            return Results.CreatedAtRoute("GetWorkoutPlanById", new { id = result.Id }, result);
        })
        .WithName("CreateWorkoutPlan")
        .WithSummary("Creates a new workout plan for the authenticated user");

        // NOVO ENDPOINT: Listar todos os planos do usuário
        group.MapGet("/", async (ClaimsPrincipal user, ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetAllWorkoutPlansQuery(ownerId);
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetAllWorkoutPlans")
        .WithSummary("Gets all workout plans for the authenticated user");

        // Endpoint de BUSCA POR ID (agora com os usings corretos)
        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetWorkoutPlanByIdQuery(id, ownerId);
            var result = await sender.Send(query);

            return result is not null
                ? Results.Ok(result)
                : Results.NotFound();
        })
          .WithName("GetWorkoutPlanById")
          .WithSummary("Gets a specific workout plan by its ID, including its exercises.");

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateWorkoutPlanRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new UpdateWorkoutPlanCommand(id, request.Name, request.Goal, ownerId);

            try
            {
                await sender.Send(command);
                // 204 No Content é a resposta padrão para um update bem-sucedido que não retorna dados.
                return Results.NoContent();
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("UpdateWorkoutPlan")
        .WithSummary("Updates a specific workout plan");

        group.MapDelete("/{id:guid}", async (
            Guid id,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new DeleteWorkoutPlanCommand(id, ownerId);

            try
            {
                await sender.Send(command);
                // Assim como no Update, 204 No Content é a resposta ideal para um delete bem-sucedido.
                return Results.NoContent();
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("DeleteWorkoutPlan")
        .WithSummary("Deletes a specific workout plan");

        group.MapPost("/{workoutPlanId:guid}/exercises", async (
            Guid workoutPlanId,
            [FromBody] AddExerciseToPlanRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new AddExerciseToPlanCommand(
                workoutPlanId,
                ownerId,
                request.ExerciseId,
                request.Order,
                request.TargetSets,
                request.TargetReps,
                request.TargetLoad
            );

            try
            {
                var workoutExerciseId = await sender.Send(command);
                return Results.Created($"/api/workout-plans/{workoutPlanId}/exercises/{workoutExerciseId}", new { id = workoutExerciseId });
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("AddExerciseToPlan")
        .WithSummary("Adds an existing exercise to a specific workout plan");

        group.MapDelete("/{workoutPlanId:guid}/exercises/{workoutExerciseId:guid}", async (
            Guid workoutPlanId,
            Guid workoutExerciseId,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new RemoveExerciseFromPlanCommand(workoutPlanId, workoutExerciseId, ownerId);

            try
            {
                await sender.Send(command);
                return Results.NoContent();
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("RemoveExerciseFromPlan")
        .WithSummary("Removes a specific exercise from a workout plan");
        
        group.MapPost("/{planId:guid}/clone", async (
            Guid planId,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var newOwnerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new CloneWorkoutPlanCommand(planId, newOwnerId);

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
        .WithName("CloneWorkoutPlan")
        .WithSummary("Cria uma cópia de um plano de treino existente para o utilizador autenticado.");

        group.MapPatch("/{id:guid}/activate", async (
            Guid id,
            ClaimsPrincipal user,
            IApplicationDbContext context) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // First, deactivate all plans for this user
            var userPlans = await context.WorkoutPlans
                .Where(p => p.OwnerId == userId)
                .ToListAsync();

            foreach (var plan in userPlans)
            {
                plan.IsActive = false;
            }

            // Then activate the specified plan
            var targetPlan = userPlans.FirstOrDefault(p => p.Id == id);
            if (targetPlan == null)
            {
                return Results.NotFound(new { message = "Plano não encontrado" });
            }

            targetPlan.IsActive = true;
            await context.SaveChangesAsync(CancellationToken.None);

            return Results.NoContent();
        })
        .WithName("ActivateWorkoutPlan")
        .WithSummary("Ativa um plano de treino específico e desativa todos os outros do usuário");

        group.MapPatch("/{id:guid}/deactivate", async (
            Guid id,
            ClaimsPrincipal user,
            IApplicationDbContext context) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var targetPlan = await context.WorkoutPlans
                .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

            if (targetPlan == null)
            {
                return Results.NotFound(new { message = "Plano não encontrado" });
            }

            targetPlan.IsActive = false;
            await context.SaveChangesAsync(CancellationToken.None);

            return Results.NoContent();
        })
        .WithName("DeactivateWorkoutPlan")
        .WithSummary("Desativa um plano de treino específico");

        // Add a workout (day) to a plan
        group.MapPost("/{planId:guid}/workouts", async (
            Guid planId,
            [FromBody] AddWorkoutToPlanRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new AddWorkoutToPlanCommand(
                planId,
                ownerId,
                request.Name,
                request.DayOfWeek,
                request.Order
            );

            try
            {
                var workoutId = await sender.Send(command);
                return Results.Created($"/api/workout-plans/{planId}/workouts/{workoutId}", new { id = workoutId });
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("AddWorkoutToPlan")
        .WithSummary("Adds a workout day to a workout plan");

        // Add an exercise to a specific workout (day)
        group.MapPost("/{planId:guid}/workouts/{workoutId:guid}/exercises", async (
            Guid planId,
            Guid workoutId,
            [FromBody] AddExerciseToWorkoutRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new AddExerciseToWorkoutCommand(
                workoutId,
                ownerId,
                request.ExerciseId,
                request.Order,
                request.TargetSets,
                request.TargetReps,
                request.TargetLoad
            );

            try
            {
                var workoutExerciseId = await sender.Send(command);
                return Results.Created($"/api/workout-plans/{planId}/workouts/{workoutId}/exercises/{workoutExerciseId}", new { id = workoutExerciseId });
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("AddExerciseToWorkout")
        .WithSummary("Adds an exercise to a specific workout day");

        // Share workout plan with friends
        group.MapPost("/{planId:guid}/share", async (
            Guid planId,
            [FromBody] ShareWorkoutPlanRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var sharerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new ShareWorkoutPlanCommand(planId, sharerId, request.FriendIds);

            try
            {
                await sender.Send(command);
                return Results.Ok(new { message = "Treino compartilhado com sucesso!" });
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (BadRequestException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("ShareWorkoutPlan")
        .WithSummary("Shares a workout plan with friends");
    }
}