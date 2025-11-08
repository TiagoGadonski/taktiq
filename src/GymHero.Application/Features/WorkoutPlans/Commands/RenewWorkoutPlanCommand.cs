using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public record RenewWorkoutPlanCommand(
    Guid PlanId,
    Guid OwnerId,
    int AdditionalWeeks
) : IRequest<bool>;
