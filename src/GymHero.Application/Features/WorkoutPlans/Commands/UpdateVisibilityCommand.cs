using GymHero.Shared.Enums;
using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public record UpdateWorkoutPlanVisibilityCommand(
    Guid PlanId,
    Guid UserId,
    VisibilityLevel VisibilityLevel,
    bool AllowCopying
) : IRequest<bool>;
