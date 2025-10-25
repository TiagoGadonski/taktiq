using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

// O comando para deletar é simples: precisa do ID do plano e do ID do dono para segurança.
public record DeleteWorkoutPlanCommand(Guid Id, Guid OwnerId) : IRequest;