using MediatR;
namespace GymHero.Application.Features.Challenges.Commands;

public record CreateCustomChallengeCommand(
    Guid CreatorId, // ID do utilizador que está a criar (do token)
    string Title,
    string Type,
    double TargetValue,
    DateTime StartDate,
    DateTime EndDate,
    List<Guid> FriendIds) : IRequest<Guid>; // Retorna o ID do novo desafio