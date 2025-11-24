using GymHero.Application.Common.Interfaces;
using GymHero.Application.Features.WorkoutPlans.Queries;
using DomainVisibility = GymHero.Domain.Enums.VisibilityLevel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Handlers;

public class GetPublicWorkoutPlansByUserHandler : IRequestHandler<GetPublicWorkoutPlansByUserQuery, List<PublicWorkoutPlanDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPublicWorkoutPlansByUserHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PublicWorkoutPlanDto>> Handle(GetPublicWorkoutPlansByUserQuery request, CancellationToken cancellationToken)
    {
        // Query for public plans from specific user
        var query = _context.WorkoutPlans
            .Include(p => p.Owner)
            .Include(p => p.Workouts)
            .Where(p => p.OwnerId == request.UserId &&
                       p.VisibilityLevel == DomainVisibility.Public &&
                       p.IsPublic);

        // Order by most recently published
        query = query.OrderByDescending(p => p.PublishedAt);

        // Apply page size limit
        var plans = await query
            .Take(request.PageSize)
            .Select(p => new PublicWorkoutPlanDto(
                p.Id,
                p.Name,
                p.Description,
                p.Goal,
                p.Duration,
                p.Owner.Name,
                p.OwnerId,
                p.PublishedAt ?? DateTime.UtcNow,
                p.ViewCount,
                p.AllowCopying,
                p.Workouts.Count
            ))
            .ToListAsync(cancellationToken);

        return plans;
    }
}
