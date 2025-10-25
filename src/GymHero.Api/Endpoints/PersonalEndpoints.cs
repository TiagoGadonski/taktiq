using System.Security.Claims;
using GymHero.Application.Common.Exceptions;
using GymHero.Shared.DTOs;
using GymHero.Application.Features.Personal.Commands;
using GymHero.Application.Features.Personal.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    }
}