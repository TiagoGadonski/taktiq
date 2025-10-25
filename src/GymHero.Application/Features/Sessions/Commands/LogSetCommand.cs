using MediatR;
namespace GymHero.Application.Features.Sessions.Commands;

// DTO para a resposta de conclusão de exercício
public record LogSetResponse(Guid SetId, string MotivationalMessage);

public record LogSetCommand(
    Guid SessionId,
    Guid OwnerId,
    Guid ExerciseId,
    int SetNumber,
    int? Reps,
    double? Load,
    int? Rpe) : IRequest<LogSetResponse>; // Retorna o ID e mensagem motivacional