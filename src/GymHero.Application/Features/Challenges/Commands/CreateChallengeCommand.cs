using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Challenges.Commands;

public record CreateChallengeCommand(
    Guid OwnerId,
    string Title,
    string Type,
    double TargetValue,
    DateTime StartDate,
    DateTime EndDate,
    ChallengeTargetType TargetType,
    bool IsDefault,
    string? IconName = null) : IRequest<ChallengeResponse>;