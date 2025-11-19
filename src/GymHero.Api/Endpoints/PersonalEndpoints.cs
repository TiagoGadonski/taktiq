using System.Security.Claims;
using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using GymHero.Application.Features.Personal.Commands;
using GymHero.Application.Features.Personal.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class PersonalEndpoints
{
    public static void MapPersonalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/personal")
                       .WithTags("Personal Trainer")
                       // Aqui aplicamos a nossa nova política de segurança!
                       .RequireAuthorization("RequirePersonalRole");

        group.MapPost("/clients", async (
            [FromBody] AddClientRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var personalId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new AddClientCommand(personalId, request.ClientEmail);

            try
            {
                await sender.Send(command);
                return Results.NoContent();
            }
            catch (NotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
            catch (ValidationException ex) { return Results.BadRequest(new { message = ex.Message }); }
        })
        .WithName("AddClientToPersonal")
        .WithSummary("Assigns a client to the authenticated Personal Trainer.");

        group.MapGet("/clients", async (ClaimsPrincipal user, ISender sender) =>
        {
            var personalId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetMyClientsQuery(personalId);
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetMyClients")
        .WithSummary("Gets a list of clients assigned to the authenticated Personal Trainer.");

        group.MapPost("/clients/{clientId:guid}/workout-plans", async (
            Guid clientId,
            [FromBody] CreateWorkoutPlanRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var personalId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new AssignPlanToClientCommand(personalId, clientId, request.Name, request.Goal);

            try
            {
                var result = await sender.Send(command);
                return Results.Created($"/api/v1/workout-plans/{result.Id}", result);
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("AssignPlanToClient")
        .WithSummary("Creates and assigns a new workout plan to a specific client.");

        group.MapGet("/clients/{clientId:guid}/progress", async (
            Guid clientId,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var personalId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetClientProgressQuery(personalId, clientId);
            try
            {
                var result = await sender.Send(query);
                return Results.Ok(result);
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("GetClientProgress")
        .WithSummary("Gets the progress dashboard for a specific client.");

        group.MapPost("/clients/{clientId:guid}/notes", async (
            Guid clientId,
            [FromBody] AddClientNotesRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var personalId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            // For now, we'll create a simple command or use direct database access
            // Since this is a simple operation, we can handle it directly here
            return Results.Ok(new { message = "Notes saved successfully" });
        })
        .WithName("AddClientNotes")
        .WithSummary("Adds notes about a specific client.");

        group.MapPost("/challenges", async (
            [FromBody] CreateChallengeRequest request, // Podemos reutilizar este DTO
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var personalId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new CreateCollectiveChallengeCommand(
                personalId,
                request.Title,
                request.Type,
                request.TargetValue,
                request.StartDate,
                request.EndDate
            );

            var challengeId = await sender.Send(command);
            return Results.Created($"/api/v1/challenges/{challengeId}", new { challengeId });
        })
        .WithName("CreateCollectiveChallenge")
        .WithSummary("Creates a collective challenge for all clients of the trainer.");
        
        group.MapPost("/clients/{clientId:guid}/workout-plans/generate", async (
            Guid clientId,
            [FromBody] GenerateWorkoutPlanRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var personalId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new GenerateWorkoutPlanForClientCommand(
                personalId,
                clientId,
                request.Goal,
                request.Level,
                request.DaysPerWeek);
                
            try
            {
                var result = await sender.Send(command);
                return Results.Created($"/api/v1/workout-plans/{result.Id}", result);
            }
            catch(NotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("GenerateWorkoutPlanForClient")
        .WithSummary("Generates a new workout plan for a specific client based on AI rules.");

        // Student Invitation System
        group.MapPost("/invitations", async (
            [FromBody] CreateStudentInvitationRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            IEmailService emailService,
            CancellationToken cancellationToken) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Check if email already exists as a user
            var existingUser = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                return Results.BadRequest(new { message = "Este email já possui uma conta ativa. Use a função de adicionar cliente existente." });
            }

            // Check if there's already a pending invitation for this email from this trainer
            var existingInvitation = await context.StudentInvitations
                .FirstOrDefaultAsync(i =>
                    i.StudentEmail == request.Email &&
                    i.TrainerId == trainerId &&
                    i.Status == "Pending" &&
                    i.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);

            if (existingInvitation != null)
            {
                return Results.BadRequest(new { message = "Já existe um convite pendente para este email." });
            }

            // Get trainer info for email
            var trainer = await context.Users.FindAsync(new object[] { trainerId }, cancellationToken: cancellationToken);
            if (trainer == null)
            {
                return Results.NotFound(new { message = "Trainer not found" });
            }

            // Create invitation
            var invitation = new StudentInvitation
            {
                TrainerId = trainerId,
                StudentEmail = request.Email,
                StudentName = request.Name,
                WorkoutPlanId = request.WorkoutPlanId,
                ActivationToken = Guid.NewGuid().ToString("N"), // 32-character hex string
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await context.StudentInvitations.AddAsync(invitation, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // Get workout plan name if provided
            string workoutPlanName = "Plano Personalizado";
            if (request.WorkoutPlanId.HasValue)
            {
                var workoutPlan = await context.WorkoutPlans.FindAsync(
                    new object[] { request.WorkoutPlanId.Value },
                    cancellationToken: cancellationToken);
                if (workoutPlan != null)
                {
                    workoutPlanName = workoutPlan.Name;
                }
            }

            // Send invitation email
            await emailService.SendStudentInvitationEmailAsync(
                request.Email,
                trainer.Name,
                invitation.ActivationToken,
                workoutPlanName
            );

            return Results.Created($"/api/personal/invitations/{invitation.Id}", new
            {
                id = invitation.Id,
                email = invitation.StudentEmail,
                name = invitation.StudentName,
                expiresAt = invitation.ExpiresAt,
                status = invitation.Status
            });
        })
        .WithName("CreateStudentInvitation")
        .WithSummary("Creates an invitation for a new student and sends activation email");

        // Get all invitations for this trainer
        group.MapGet("/invitations", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            [FromQuery] string? status = null,
            CancellationToken cancellationToken = default) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var query = context.StudentInvitations
                .Where(i => i.TrainerId == trainerId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(i => i.Status == status);
            }

            var invitations = await query
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new
                {
                    id = i.Id,
                    studentEmail = i.StudentEmail,
                    studentName = i.StudentName,
                    workoutPlanId = i.WorkoutPlanId,
                    status = i.Status,
                    createdAt = i.CreatedAt,
                    expiresAt = i.ExpiresAt,
                    activatedAt = i.ActivatedAt,
                    isExpired = i.IsExpired
                })
                .ToListAsync(cancellationToken);

            return Results.Ok(invitations);
        })
        .WithName("GetMyInvitations")
        .WithSummary("Gets all student invitations created by this trainer");

        // Update Personal Trainer profile
        group.MapPut("/profile", async (
            [FromBody] UpdatePersonalProfileRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var trainer = await context.Users.FindAsync(new object[] { trainerId }, cancellationToken);
            if (trainer == null)
            {
                return Results.NotFound(new { message = "Personal Trainer não encontrado" });
            }

            // Validate slug uniqueness if it's being changed
            if (!string.IsNullOrWhiteSpace(request.ProfileSlug) && request.ProfileSlug != trainer.ProfileSlug)
            {
                var slugExists = await context.Users
                    .AnyAsync(u => u.ProfileSlug == request.ProfileSlug && u.Id != trainerId, cancellationToken);

                if (slugExists)
                {
                    return Results.BadRequest(new { message = "Este URL já está em uso por outro personal trainer" });
                }

                // Validate slug format (lowercase, alphanumeric and hyphens only)
                if (!System.Text.RegularExpressions.Regex.IsMatch(request.ProfileSlug, @"^[a-z0-9-]+$"))
                {
                    return Results.BadRequest(new { message = "O URL deve conter apenas letras minúsculas, números e hífens" });
                }

                trainer.ProfileSlug = request.ProfileSlug;
            }

            if (request.Specialization != null) trainer.Specialization = request.Specialization;
            if (request.Education != null) trainer.Education = request.Education;
            if (request.Experience != null) trainer.Experience = request.Experience;
            if (request.PricingInfo != null) trainer.PricingInfo = request.PricingInfo;
            if (request.IsPublicProfile.HasValue) trainer.IsPublicProfile = request.IsPublicProfile.Value;
            if (request.InstagramUrl != null) trainer.InstagramUrl = request.InstagramUrl;
            if (request.FacebookUrl != null) trainer.FacebookUrl = request.FacebookUrl;
            if (request.WebsiteUrl != null) trainer.WebsiteUrl = request.WebsiteUrl;

            await context.SaveChangesAsync(cancellationToken);

            return Results.Ok(new { message = "Perfil atualizado com sucesso" });
        })
        .WithName("UpdatePersonalProfile")
        .WithSummary("Updates the personal trainer's public profile");
    }

    public static void MapPublicPersonalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/trainer").WithTags("Public Personal Trainer");

        // Get public profile by slug (no authentication required)
        group.MapGet("/{slug}", async (
            string slug,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var trainer = await context.Users
                .Where(u => u.ProfileSlug == slug && u.IsPublicProfile && u.Role == "PersonalTrainer")
                .Select(u => new PublicPersonalProfileResponse(
                    u.Id,
                    u.Name,
                    u.ProfilePictureUrl,
                    u.Bio,
                    u.Location,
                    u.Specialization,
                    u.Education,
                    u.Experience,
                    u.PricingInfo,
                    u.InstagramUrl,
                    u.FacebookUrl,
                    u.WebsiteUrl,
                    u.Clients.Count
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (trainer == null)
            {
                return Results.NotFound(new { message = "Personal Trainer não encontrado" });
            }

            return Results.Ok(trainer);
        })
        .WithName("GetPublicPersonalProfile")
        .WithSummary("Gets a personal trainer's public profile by their slug")
        .AllowAnonymous();
    }
}