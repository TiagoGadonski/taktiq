using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Challenges.Queries;

public record GetMyChallengesQuery(Guid UserId) : IRequest<IEnumerable<ChallengeResponse>>;