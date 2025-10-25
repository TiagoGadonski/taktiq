using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Ranking.Queries;

public class GetUsersRankingQueryHandler : IRequestHandler<GetUsersRankingQuery, IEnumerable<RankingUserResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersRankingQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<RankingUserResponse>> Handle(GetUsersRankingQuery request, CancellationToken cancellationToken)
    {
        // 1. Definir o período de tempo (últimos 30 dias)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        // 2. Fazer a consulta para calcular a pontuação
        var userScores = await _context.Users
            .Select(user => new 
            {
                UserId = user.Id,
                UserName = user.Name,
                // Para cada utilizador, contamos quantas sessões de treino ele completou no período
                Score = user.WorkoutPlans
                    .SelectMany(plan => plan.WorkoutSessions)
                    .Count(session => session.CompletedAt != null && session.CompletedAt >= thirtyDaysAgo)
            })
            // 3. Ordenar os utilizadores pela pontuação, do maior para o menor
            .OrderByDescending(x => x.Score)
            // Opcional: Limitar ao Top 100, por exemplo, para performance
            .Take(100) 
            .ToListAsync(cancellationToken);

        // 4. Transformar os resultados no nosso DTO de resposta, adicionando a posição (Rank)
        var rankingResponse = userScores.Select((userScore, index) => new RankingUserResponse(
            Rank: index + 1, // O índice começa em 0, então adicionamos 1
            UserId: userScore.UserId,
            UserName: userScore.UserName,
            Score: userScore.Score
        ));

        return rankingResponse;
    }
}