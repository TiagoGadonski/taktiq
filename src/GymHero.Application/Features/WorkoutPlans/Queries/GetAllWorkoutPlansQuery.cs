using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Queries;

// Esta query representa a solicitação para buscar todos os planos de um usuário específico.
// Ela retorna uma lista de WorkoutPlanDetailResponse com os exercícios incluídos.
public record GetAllWorkoutPlansQuery(Guid OwnerId) : IRequest<IEnumerable<WorkoutPlanDetailResponse>>;