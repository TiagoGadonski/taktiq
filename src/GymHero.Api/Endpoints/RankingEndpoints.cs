using GymHero.Application.Features.Ranking.Queries;
using MediatR;

namespace GymHero.Api.Endpoints;

public static class RankingEndpoints
{
    public static void MapRankingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ranking").WithTags("Ranking");

        // Este endpoint é público, por isso não tem .RequireAuthorization()
        group.MapGet("/", async (ISender sender) =>
        {
            var query = new GetUsersRankingQuery();
            var result = await sender.Send(query);
            return Results.Ok(result);
        });
    }
}