using GymHero.Application.Common.Interfaces;
using GymHero.Application.Features.WorkoutPlans.Queries;
using DomainVisibility = GymHero.Domain.Enums.VisibilityLevel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.WorkoutPlans.Handlers;

public class GetPublicWorkoutPlansHandler : IRequestHandler<GetPublicWorkoutPlansQuery, GetPublicWorkoutPlansResponse>
{
    private readonly IApplicationDbContext _context;

    public GetPublicWorkoutPlansHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetPublicWorkoutPlansResponse> Handle(GetPublicWorkoutPlansQuery request, CancellationToken cancellationToken)
    {
        // Base query for public plans
        var query = _context.WorkoutPlans
            .Include(p => p.Owner)
            .Include(p => p.Workouts)
            .Where(p => p.VisibilityLevel == DomainVisibility.Public && p.IsPublic);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                p.Owner.Name.ToLower().Contains(searchLower)
            );
        }

        // Apply goal filter
        if (!string.IsNullOrWhiteSpace(request.Goal))
        {
            query = query.Where(p => p.Goal != null && p.Goal.ToLower().Contains(request.Goal.ToLower()));
        }

        // Order by most recently published
        query = query.OrderByDescending(p => p.PublishedAt);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var plans = await query
            .Skip((request.Page - 1) * request.PageSize)
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

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return new GetPublicWorkoutPlansResponse(
            plans,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );
    }
}
