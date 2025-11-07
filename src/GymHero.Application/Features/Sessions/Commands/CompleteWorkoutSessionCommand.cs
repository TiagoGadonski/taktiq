using MediatR;
namespace GymHero.Application.Features.Sessions.Commands;

// DTO para a resposta de conclusão de treino
public record CompleteWorkoutSessionResponse(string MotivationalMessage);

// Comando para finalizar a sessão. Inclui IDs para validação e notas opcionais.
public record CompleteWorkoutSessionCommand(Guid SessionId, Guid OwnerId, string? Notes) : IRequest<CompleteWorkoutSessionResponse>;