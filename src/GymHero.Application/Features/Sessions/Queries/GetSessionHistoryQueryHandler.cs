using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Sessions.Queries;

public class GetSessionHistoryQueryHandler : IRequestHandler<GetSessionHistoryQuery, PaginatedResponse<WorkoutSessionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSessionHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<WorkoutSessionDto>> Handle(GetSessionHistoryQuery request, CancellationToken cancellationToken)
    {
        // WorkoutSession doesn't have OwnerId directly - it's accessed through WorkoutPlan
        var query = _context.WorkoutSessions
            .Include(s => s.WorkoutPlan)
            .Include(s => s.Sets)
                .ThenInclude(set => set.Exercise)
            .Where(s => s.CompletedAt != null)
            .Where(s => s.WorkoutPlan == null || s.WorkoutPlan.OwnerId == request.UserId);

        // Apply date filters if provided
        if (request.StartDate.HasValue)
        {
            query = query.Where(s => s.StartedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(s => s.StartedAt <= request.EndDate.Value);
        }

        // Order by most recent first
        query = query.OrderByDescending(s => s.StartedAt);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var sessions = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new WorkoutSessionDto
            {
                Id = s.Id,
                WorkoutPlanId = s.WorkoutPlanId,
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt,
                Sets = s.Sets.Select(set => new WorkoutSetDto
                {
                    Id = set.Id,
                    ExerciseId = set.ExerciseId,
                    ExerciseName = set.Exercise != null ? set.Exercise.Name : "",
                    SetNumber = set.SetNumber,
                    Reps = set.Reps,
                    Load = set.Load,
                    Rpe = set.Rpe
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<WorkoutSessionDto>
        {
            Data = sessions,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }
}
