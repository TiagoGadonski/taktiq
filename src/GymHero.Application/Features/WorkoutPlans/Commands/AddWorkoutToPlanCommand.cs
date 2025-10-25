using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public record AddWorkoutToPlanCommand(
    Guid PlanId,
    Guid OwnerId,
    string Name,
    int? DayOfWeek,
    int Order
) : IRequest<Guid>;
