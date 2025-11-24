using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Queries;

public record GetPublicWorkoutPlansByUserQuery(
    Guid UserId,
    int PageSize = 20
) : IRequest<List<PublicWorkoutPlanDto>>;
