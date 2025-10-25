using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Challenges.Commands;

public record CreateChallengeCommand(
    Guid OwnerId,
    string Title,
    string Type,
    double TargetValue,
    DateTime StartDate,
    DateTime EndDate) : IRequest<ChallengeResponse>;