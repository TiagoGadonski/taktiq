using System.Security.Claims;
using GymHero.Shared.DTOs;
using GymHero.Application.Features.Progress.Commands;
using GymHero.Application.Features.Progress.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using GymHero.Application.DTOs;

namespace GymHero.Api.Endpoints;

public static class ProgressEndpoints
{
    public static void MapProgressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/progress")
                       .WithTags("Progress Tracking")
                       .RequireAuthorization();

        // Endpoint para obter o dashboard de progresso
        group.MapGet("/dashboard", async (ClaimsPrincipal user, ISender sender) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetDashboardQuery(userId);
            var dashboard = await sender.Send(query);
            return Results.Ok(dashboard);
        })
        .WithName("GetProgressDashboard")
        .WithSummary("Gets the progress dashboard for the authenticated user");

        // Endpoint para obter o histórico de métricas
        group.MapGet("/", async (ClaimsPrincipal user, ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetProgressHistoryQuery(ownerId);
            var result = await sender.Send(query);
            return Results.Ok(result);
        });

        // Endpoint para registar uma nova métrica
        group.MapPost("/", async (
            [FromBody] LogProgressMetricRequest request,
            ClaimsPrincipal user,
            ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var command = new LogProgressMetricCommand(ownerId, request.Type, request.Value, request.Unit, request.Date);
            var result = await sender.Send(command);
            return Results.Created($"/api/v1/progress/{result.Id}", result);
        });

        group.MapGet("/prs", async (ClaimsPrincipal user, ISender sender) =>
        {
            var ownerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var query = new GetUserPersonalRecordsQuery(ownerId);
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetUserPersonalRecords")
        .WithSummary("Gets all personal records for the authenticated user");
    }
}