using MediatR;
namespace GymHero.Application.Features.Sessions.Commands;

// Comando para finalizar a sessão. Só precisa dos IDs para validação.
public record CompleteWorkoutSessionCommand(Guid SessionId, Guid OwnerId) : IRequest;