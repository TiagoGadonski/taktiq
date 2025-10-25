using MediatR;
namespace GymHero.Application.Features.Personal.Commands;

public record CreateCollectiveChallengeCommand(
    Guid PersonalId,
    string Title,
    string Type,
    double TargetValue,
    DateTime StartDate,
    DateTime EndDate) : IRequest<Guid>; // Retorna o ID do novo desafio