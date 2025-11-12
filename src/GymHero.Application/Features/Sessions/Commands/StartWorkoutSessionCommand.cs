using MediatR;
namespace GymHero.Application.Features.Sessions.Commands;

// O comando precisa do ID do plano (opcional para treino livre), ID do treino específico, e do ID do dono para iniciar a sessão.
// Ele retornará o ID da nova sessão criada.
public record StartWorkoutSessionCommand(Guid? WorkoutPlanId, Guid? WorkoutId, Guid OwnerId) : IRequest<Guid>;