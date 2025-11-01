using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Queries;

public record GetPublicWorkoutPlansQuery(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    string? Goal = null
) : IRequest<GetPublicWorkoutPlansResponse>;

public record PublicWorkoutPlanDto(
    Guid Id,
    string Name,
    string? Description,
    string? Goal,
    int? Duration,
    string CreatorName,
    Guid CreatorId,
    DateTime PublishedAt,
    int ViewCount,
    bool AllowCopying,
    int WorkoutCount
);

public record GetPublicWorkoutPlansResponse(
    List<PublicWorkoutPlanDto> Plans,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
