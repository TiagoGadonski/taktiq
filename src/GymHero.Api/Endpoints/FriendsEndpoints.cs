using System.Security.Claims;
using GymHero.Application.Common.Exceptions;
using GymHero.Application.Features.Friends.Commands;
using GymHero.Application.Features.Friends.Queries;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymHero.Api.Endpoints;

public static class FriendsEndpoints
{
    public static void MapFriendsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/friends")
                       .WithTags("Friends")
                       .RequireAuthorization();

        group.MapPost("/requests", async (
            [FromBody] SendFriendRequestRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new SendFriendRequestCommand(requesterId, request.AddresseeEmail);

            try
            {
                await sender.Send(command);
                return Results.Ok("Pedido de amizade enviado com sucesso.");
            }
            catch (NotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
            catch (ValidationException ex) { return Results.BadRequest(new { message = ex.Message }); }
        })
        .WithName("SendFriendRequest")
        .WithSummary("Envia um pedido de amizade para outro utilizador.");

        group.MapGet("/requests/pending", async (ClaimsPrincipal user, ISender sender) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetPendingFriendRequestsQuery(userId);
            var result = await sender.Send(query);
            return Results.Ok(result);
        });

        // NOVO: Endpoint para RESPONDER a um pedido
        group.MapPut("/requests/{friendshipId:guid}", async (
            Guid friendshipId,
            [FromBody] RespondToFriendRequestRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var currentUserId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new RespondToFriendRequestCommand(currentUserId, friendshipId, request.Accept);
            try
            {
                await sender.Send(command);
                return Results.NoContent();
            }
            catch (NotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
            catch (ValidationException ex) { return Results.BadRequest(new { message = ex.Message }); }
        });

        group.MapGet("/", async (ClaimsPrincipal user, ISender sender) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetFriendsQuery(userId);
            var result = await sender.Send(query);
            return Results.Ok(result);
        });

        // NOVO: Endpoint para REMOVER um amigo
        group.MapDelete("/{friendshipId:guid}", async (Guid friendshipId, ClaimsPrincipal user, ISender sender) =>
        {
            var currentUserId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new RemoveFriendCommand(currentUserId, friendshipId);

            try
            {
                await sender.Send(command);
                return Results.NoContent();
            }
            catch (NotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
            catch (ValidationException ex) { return Results.Forbid(); } // Forbid é mais apropriado para erros de permissão
        });
        var requestsGroup = group.MapGroup("/requests");

        group.MapGet("/search", async (string query, ClaimsPrincipal user, ISender sender) =>
        {
            var currentUserId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var searchQuery = new SearchUsersQuery(query, currentUserId);
            var result = await sender.Send(searchQuery);
            return Results.Ok(result);
        });

        requestsGroup.MapPost("/{addresseeId:guid}", async (
            Guid addresseeId,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new SendFriendRequestByIdCommand(requesterId, addresseeId);

            try
            {
                await sender.Send(command);
                return Results.Ok("Pedido de amizade enviado com sucesso.");
            }
            catch (NotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
            catch (ValidationException ex) { return Results.BadRequest(new { message = ex.Message }); }
        });

        

    }
}