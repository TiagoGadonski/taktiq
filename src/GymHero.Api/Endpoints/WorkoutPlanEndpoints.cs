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
            var command = new CreateWorkoutPlanCommand(request.Name, request.Goal, ownerId, request.Duration);
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

        // Renew/extend a workout plan
        group.MapPost("/{id:guid}/renew", async (
            Guid id,
            [FromBody] RenewWorkoutPlanRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new RenewWorkoutPlanCommand(id, ownerId, request.AdditionalWeeks);

            var result = await sender.Send(command);

            return result
                ? Results.Ok(new { message = "Plano renovado com sucesso!" })
                : Results.NotFound(new { message = "Plano não encontrado" });
        })
        .WithName("RenewWorkoutPlan")
        .WithSummary("Renews/extends a workout plan by adding more weeks");

        // Duplicate a workout plan
        group.MapPost("/{id:guid}/duplicate", async (
            Guid id,
            [FromBody] DuplicateWorkoutPlanRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new DuplicateWorkoutPlanCommand(id, ownerId, request.Duration);

            try
            {
                var result = await sender.Send(command);
                return Results.Created($"/api/workout-plans/{result.Id}", result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("DuplicateWorkoutPlan")
        .WithSummary("Creates a new workout plan based on an existing one");

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

        // Delete a workout (day) from a plan
        group.MapDelete("/{planId:guid}/workouts/{workoutId:guid}", async (
            Guid planId,
            Guid workoutId,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new DeleteWorkoutCommand(workoutId, ownerId);

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
        .WithName("DeleteWorkout")
        .WithSummary("Deletes a workout day from a workout plan");

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

        // Replace an exercise in a workout
        group.MapPut("/{planId:guid}/workouts/{workoutId:guid}/exercises/{workoutExerciseId:guid}", async (
            Guid planId,
            Guid workoutId,
            Guid workoutExerciseId,
            [FromBody] ReplaceExerciseRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new ReplaceExerciseInWorkoutCommand(
                workoutExerciseId,
                ownerId,
                request.NewExerciseId,
                request.TargetSets,
                request.TargetReps,
                request.TargetLoad
            );

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
        .WithName("ReplaceExerciseInWorkout")
        .WithSummary("Replaces an exercise in a workout with a new one");

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

        // Update workout plan visibility settings
        group.MapPatch("/{planId:guid}/visibility", async (
            Guid planId,
            [FromBody] UpdateVisibilityRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new UpdateWorkoutPlanVisibilityCommand(
                planId,
                userId,
                request.VisibilityLevel,
                request.AllowCopying
            );

            var result = await sender.Send(command);
            return result
                ? Results.Ok(new { message = "Visibilidade atualizada com sucesso!" })
                : Results.NotFound(new { message = "Plano não encontrado" });
        })
        .WithName("UpdateWorkoutPlanVisibility")
        .WithSummary("Updates the visibility settings of a workout plan");

        // Browse public workout plans (no auth required)
        var publicGroup = app.MapGroup("/api/workout-plans/public")
            .WithTags("Public Workout Plans")
            .AllowAnonymous();

        publicGroup.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            [FromQuery] string? goal,
            ISender sender) =>
        {
            var query = new GetPublicWorkoutPlansQuery(
                page > 0 ? page : 1,
                pageSize > 0 && pageSize <= 50 ? pageSize : 20,
                search,
                goal
            );

            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("BrowsePublicWorkoutPlans")
        .WithSummary("Browse all public workout plans with optional search and filtering");

        publicGroup.MapGet("/{planId:guid}", async (
            Guid planId,
            ISender sender) =>
        {
            var query = new GetPublicWorkoutPlanByIdQuery(planId);
            var result = await sender.Send(query);

            return result is not null
                ? Results.Ok(result)
                : Results.NotFound(new { message = "Plano público não encontrado" });
        })
        .WithName("GetPublicWorkoutPlanDetail")
        .WithSummary("Get details of a public workout plan (increments view count)");

        publicGroup.MapGet("/user/{userId:guid}", async (
            Guid userId,
            [FromQuery] int pageSize,
            ISender sender) =>
        {
            var query = new GetPublicWorkoutPlansByUserQuery(
                userId,
                pageSize > 0 && pageSize <= 50 ? pageSize : 20
            );

            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetPublicWorkoutPlansByUser")
        .WithSummary("Get public workout plans created by a specific user");

        // Alternative endpoint for user-specific public plans
        var userPublicGroup = app.MapGroup("/api/workout-plans/user")
            .WithTags("Public Workout Plans")
            .AllowAnonymous();

        userPublicGroup.MapGet("/{userId:guid}/public", async (
            Guid userId,
            [FromQuery] int pageSize,
            ISender sender) =>
        {
            var query = new GetPublicWorkoutPlansByUserQuery(
                userId,
                pageSize > 0 && pageSize <= 50 ? pageSize : 20
            );

            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetPublicWorkoutPlansByUserAlternate")
        .WithSummary("Get public workout plans created by a specific user (alternative route)");

        // Marketplace endpoints
        var marketplaceGroup = app.MapGroup("/api/marketplace")
            .WithTags("Marketplace")
            .AllowAnonymous();

        marketplaceGroup.MapGet("/plans", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            [FromQuery] string? goal,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            IApplicationDbContext context) =>
        {
            var pageNumber = page > 0 ? page : 1;
            var size = pageSize > 0 && pageSize <= 50 ? pageSize : 20;

            var query = context.WorkoutPlans
                .Include(p => p.Owner)
                .Include(p => p.Workouts)
                    .ThenInclude(w => w.Exercises)
                .Where(p => p.ForSale && p.IsPublic);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                    (p.Goal != null && p.Goal.ToLower().Contains(searchLower))
                );
            }

            // Apply goal filter
            if (!string.IsNullOrWhiteSpace(goal))
            {
                var goalLower = goal.ToLower();
                query = query.Where(p => p.Goal != null && p.Goal.ToLower().Contains(goalLower));
            }

            // Apply price filters
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)size);

            var plans = await query
                .OrderByDescending(p => p.ViewCount)
                .Skip((pageNumber - 1) * size)
                .Take(size)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    goal = p.Goal,
                    duration = p.Duration,
                    price = p.Price,
                    creatorId = p.OwnerId,
                    creatorName = p.Owner.Name,
                    viewCount = p.ViewCount,
                    workoutCount = p.Workouts.Count,
                    exerciseCount = p.Workouts.SelectMany(w => w.Exercises).Count(),
                    publishedAt = p.PublishedAt
                })
                .ToListAsync();

            return Results.Ok(new
            {
                data = plans,
                page = pageNumber,
                pageSize = size,
                totalCount,
                totalPages
            });
        })
        .WithName("GetMarketplacePlans")
        .WithSummary("Browse workout plans available for purchase");

        marketplaceGroup.MapGet("/plans/{planId:guid}", async (
            Guid planId,
            IApplicationDbContext context) =>
        {
            var plan = await context.WorkoutPlans
                .Include(p => p.Owner)
                .Include(p => p.Workouts)
                    .ThenInclude(w => w.Exercises)
                        .ThenInclude(we => we.Exercise)
                .Where(p => p.Id == planId && p.ForSale && p.IsPublic)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    goal = p.Goal,
                    duration = p.Duration,
                    price = p.Price,
                    creatorId = p.OwnerId,
                    creatorName = p.Owner.Name,
                    viewCount = p.ViewCount,
                    allowCopying = p.AllowCopying,
                    workouts = p.Workouts.OrderBy(w => w.Order).Select(w => new
                    {
                        id = w.Id,
                        name = w.Name,
                        dayOfWeek = w.DayOfWeek,
                        order = w.Order,
                        exercises = w.Exercises.OrderBy(e => e.Order).Select(e => new
                        {
                            id = e.Id,
                            exerciseId = e.ExerciseId,
                            exerciseName = e.Exercise != null ? e.Exercise.Name : "",
                            order = e.Order,
                            targetSets = e.TargetSets,
                            targetReps = e.TargetReps,
                            targetLoad = e.TargetLoad,
                            targetRpe = e.TargetRpe,
                            restSeconds = e.RestSeconds,
                            notes = e.Notes
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (plan == null)
            {
                return Results.NotFound(new { message = "Plano não encontrado ou não disponível para compra" });
            }

            // Increment view count
            var planEntity = await context.WorkoutPlans.FindAsync(planId);
            if (planEntity != null)
            {
                planEntity.ViewCount++;
                await context.SaveChangesAsync(CancellationToken.None);
            }

            return Results.Ok(plan);
        })
        .WithName("GetMarketplacePlanDetail")
        .WithSummary("Get detailed information about a marketplace plan");

        // Purchase a plan (requires authentication)
        var purchaseGroup = app.MapGroup("/api/marketplace")
            .WithTags("Marketplace")
            .RequireAuthorization();

        purchaseGroup.MapPost("/plans/{planId:guid}/purchase", async (
            Guid planId,
            ClaimsPrincipal user,
            ISender sender,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var buyerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Check if the plan requires payment
            var plan = await context.WorkoutPlans
                .FirstOrDefaultAsync(p => p.Id == planId && p.ForSale, cancellationToken);

            if (plan == null)
            {
                return Results.NotFound(new { message = "Plano não encontrado" });
            }

            // If plan has a price, redirect to payment flow
            if (plan.Price.HasValue && plan.Price.Value > 0)
            {
                return Results.BadRequest(new
                {
                    message = "Este plano requer pagamento. Use o endpoint /api/payments/create-intent.",
                    requiresPayment = true,
                    price = plan.Price.Value
                });
            }

            // For free plans, clone directly
            var command = new CloneWorkoutPlanCommand(planId, buyerId);

            try
            {
                var result = await sender.Send(command);
                return Results.Ok(new
                {
                    message = "Plano adquirido com sucesso!",
                    planId = result.Id
                });
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("PurchaseMarketplacePlan")
        .WithSummary("Purchase a free workout plan from the marketplace");

        // Update marketplace settings for a plan (PT only)
        group.MapPatch("/{planId:guid}/marketplace", async (
            Guid planId,
            [FromBody] UpdateMarketplaceSettingsRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var plan = await context.WorkoutPlans
                .FirstOrDefaultAsync(p => p.Id == planId && p.OwnerId == userId);

            if (plan == null)
            {
                return Results.NotFound(new { message = "Plano não encontrado" });
            }

            plan.ForSale = request.ForSale;
            plan.Price = request.Price;

            // If marking for sale, must be public
            if (request.ForSale && !plan.IsPublic)
            {
                return Results.BadRequest(new { message = "O plano deve ser público para ser vendido" });
            }

            await context.SaveChangesAsync(CancellationToken.None);

            return Results.Ok(new { message = "Configurações de marketplace atualizadas com sucesso" });
        })
        .WithName("UpdateMarketplaceSettings")
        .WithSummary("Updates marketplace settings for a workout plan");
    }
}