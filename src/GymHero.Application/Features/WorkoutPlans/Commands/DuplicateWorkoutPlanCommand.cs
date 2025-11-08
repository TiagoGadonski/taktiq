using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public record DuplicateWorkoutPlanCommand(
    Guid PlanId,
    Guid OwnerId,
    int Duration
) : IRequest<WorkoutPlanResponse>;
