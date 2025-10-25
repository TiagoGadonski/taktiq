using MediatR;
namespace GymHero.Application.Features.Sessions.Commands;

public record LogSetCommand(
    Guid SessionId,
    Guid OwnerId,
    Guid ExerciseId,
    int SetNumber,
    int? Reps,
    double? Load,
    int? Rpe) : IRequest<Guid>; // Retorna o ID do novo WorkoutSet