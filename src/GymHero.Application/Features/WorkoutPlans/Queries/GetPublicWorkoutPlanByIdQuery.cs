using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Queries;

/// <summary>
/// Representa uma consulta para buscar os detalhes de um plano de treino
/// de forma pública, sem verificar o dono.
/// </summary>
public record GetPublicWorkoutPlanByIdQuery(Guid PlanId) : IRequest<WorkoutPlanDetailResponse?>;