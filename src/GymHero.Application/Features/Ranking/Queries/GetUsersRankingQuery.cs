using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Ranking.Queries;

// Query simples para solicitar o ranking, sem parâmetros necessários por agora.
public record GetUsersRankingQuery : IRequest<IEnumerable<RankingUserResponse>>;