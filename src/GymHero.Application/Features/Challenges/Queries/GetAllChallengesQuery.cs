using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Challenges.Queries;

public record GetAllChallengesQuery(Guid UserId) : IRequest<IEnumerable<ChallengeWithParticipationDto>>;
