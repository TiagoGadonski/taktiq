using MediatR;

namespace GymHero.Application.Features.Challenges.Commands;

public record CompleteChallengeCommand(Guid ChallengeId, Guid UserId) : IRequest<Unit>;
