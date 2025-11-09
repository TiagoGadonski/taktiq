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
        // Build base query with filters (no includes for count performance)
        var query = _context.WorkoutSessions
            .AsNoTracking()
            .Where(s => s.CompletedAt != null)
            .Where(s => s.OwnerId == request.UserId);

        // Apply date filters if provided
        if (request.StartDate.HasValue)
        {
            query = query.Where(s => s.StartedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(s => s.StartedAt <= request.EndDate.Value);
        }

        // Get total count BEFORE any joins/includes for better performance
        var totalCount = await query.CountAsync(cancellationToken);

        // Order by most recent first and apply pagination
        var sessions = await query
            .OrderByDescending(s => s.StartedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new WorkoutSessionDto
            {
                Id = s.Id,
                WorkoutPlanId = s.WorkoutPlanId,
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt,
                Notes = s.Notes,
                Sets = s.Sets.Select(set => new WorkoutSetDto
                {
                    Id = set.Id,
                    ExerciseId = set.ExerciseId,
                    ExerciseName = set.Exercise != null ? set.Exercise.Name : "",
                    SetNumber = set.SetNumber,
                    Reps = set.Reps,
                    Load = set.Load,
                    Rpe = set.Rpe,
                    IsAddedDuringSession = set.IsAddedDuringSession
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
