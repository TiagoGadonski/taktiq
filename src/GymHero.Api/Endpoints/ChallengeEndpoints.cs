using System.Security.Claims;
using GymHero.Shared.DTOs;
using GymHero.Application.Features.Challenges.Commands;
using GymHero.Application.Features.Challenges.Queries; // <<< Adicione este using
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymHero.Api.Endpoints;

public static class ChallengeEndpoints
{
    public static void MapChallengeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/challenges")
                       .WithTags("Challenges")
                       .RequireAuthorization();

        // Endpoint GET para listar todos os desafios do utilizador
        group.MapGet("/", async (ClaimsPrincipal user, ISender sender) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetMyChallengesQuery(userId);
            var result = await sender.Send(query);
            return Results.Ok(result);
        });

        // Endpoint POST para criar um desafio (já existente)
        group.MapPost("/", async (
            [FromBody] CreateChallengeRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new CreateChallengeCommand(
                ownerId,
                request.Title,
                request.Type,
                request.TargetValue,
                request.StartDate,
                request.EndDate);

            var result = await sender.Send(command);
            return Results.Created($"/api/v1/challenges/{result.Id}", result);
        });
        
        group.MapPost("/custom", async (
            CreateCustomChallengeRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var creatorId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new CreateCustomChallengeCommand(
                creatorId,
                request.Title,
                request.Type,
                request.TargetValue,
                request.StartDate,
                request.EndDate,
                request.FriendIds
            );

            var challengeId = await sender.Send(command);
            return Results.Created($"/api/v1/challenges/{challengeId}", new { challengeId });});
    }
}