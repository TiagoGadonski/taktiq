using GymHero.Application.Features.WorkoutPlans.Queries;
using MediatR;
namespace GymHero.Api.Endpoints;

public static class PublicEndpoints
{
    public static void MapPublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public").WithTags("Public");

        // Este endpoint é uma cópia do GetWorkoutPlanById, mas sem a verificação de dono (OwnerId)
        // e não requer autorização.
        group.MapGet("/workout-plans/{planId:guid}", async (Guid planId, ISender sender) =>
        {
            // Precisamos de uma nova Query que não exija o OwnerId
            var query = new GetPublicWorkoutPlanByIdQuery(planId);
            var result = await sender.Send(query);

            return result is not null
                ? Results.Ok(result)
                : Results.NotFound();
        });
    }
}