using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public record ShareWorkoutPlanCommand(
    Guid PlanId,
    Guid SharerId,
    List<Guid> FriendIds) : IRequest;
