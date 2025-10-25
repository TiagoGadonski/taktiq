using MediatR;
namespace GymHero.Application.Features.Sessions.Commands;

// DTO para a resposta de conclusão de treino
public record CompleteWorkoutSessionResponse(string MotivationalMessage);

// Comando para finalizar a sessão. Só precisa dos IDs para validação.
public record CompleteWorkoutSessionCommand(Guid SessionId, Guid OwnerId) : IRequest<CompleteWorkoutSessionResponse>;