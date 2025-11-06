using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Progress.Queries;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardResponse>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponse> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        // Get user's account creation date
        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new { u.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken);

        var accountCreatedAt = user?.CreatedAt ?? DateTime.UtcNow;

        // Base query for completed sessions
        var completedSessionsQuery = _context.WorkoutSessions
            .AsNoTracking()
            .Where(s => s.OwnerId == request.UserId && s.CompletedAt != null);

        // Calculate total workouts (unique days with workouts) in database
        var totalWorkouts = await completedSessionsQuery
            .Select(s => s.CompletedAt!.Value.Date)
            .Distinct()
            .CountAsync(cancellationToken);

        // Calculate total unique exercises done in database
        var totalExercises = await _context.WorkoutSets
            .AsNoTracking()
            .Where(set => set.WorkoutSession.OwnerId == request.UserId &&
                         set.WorkoutSession.CompletedAt != null)
            .Select(set => set.ExerciseId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Calculate total volume (sum of all weight × reps) in database
        var totalVolume = await _context.WorkoutSets
            .AsNoTracking()
            .Where(set => set.WorkoutSession.OwnerId == request.UserId &&
                         set.WorkoutSession.CompletedAt != null &&
                         set.Load.HasValue && set.Reps.HasValue)
            .SumAsync(set => set.Load!.Value * set.Reps!.Value, cancellationToken);

        // For streak calculation, we only need completion dates
        var workoutDates = await completedSessionsQuery
            .Select(s => s.CompletedAt!.Value.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync(cancellationToken);

        var currentStreak = CalculateStreak(workoutDates);

        // Get last 7 days of workout activity
        var weeklyWorkouts = await GetWeeklyWorkouts(request.UserId, cancellationToken);

        // Get recent PRs
        var recentPRs = await GetRecentPersonalRecords(request.UserId, cancellationToken);

        return new DashboardResponse(
            TotalWorkouts: totalWorkouts,
            TotalSets: totalExercises,
            TotalVolume: totalVolume,
            CurrentStreak: currentStreak,
            WeeklyWorkouts: weeklyWorkouts,
            RecentPRs: recentPRs,
            AccountCreatedAt: accountCreatedAt
        );
    }

    private int CalculateStreak(List<DateTime> workoutDates)
    {
        if (!workoutDates.Any())
            return 0;

        var streak = 0;
        var currentDate = DateTime.UtcNow.Date;

        // Check if there's a workout today or yesterday (allow for one day gap)
        if (!workoutDates.Any() || (currentDate - workoutDates.First()).Days > 1)
            return 0;

        foreach (var date in workoutDates)
        {
            if ((currentDate - date).Days <= 1)
            {
                streak++;
                currentDate = date;
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    private async Task<List<WeeklyWorkoutDto>> GetWeeklyWorkouts(Guid userId, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var sevenDaysAgo = today.AddDays(-6);

        // Get aggregated data for the last 7 days in a single database query
        var weeklyData = await _context.WorkoutSessions
            .AsNoTracking()
            .Where(s => s.OwnerId == userId &&
                       s.CompletedAt != null &&
                       s.CompletedAt.Value.Date >= sevenDaysAgo &&
                       s.CompletedAt.Value.Date <= today)
            .GroupBy(s => s.CompletedAt!.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                SetsCompleted = g.Sum(s => s.Sets.Count)
            })
            .ToListAsync(cancellationToken);

        // Create the 7-day list with data from the database
        var weeklyWorkouts = new List<WeeklyWorkoutDto>();
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var dayData = weeklyData.FirstOrDefault(d => d.Date == date);

            weeklyWorkouts.Add(new WeeklyWorkoutDto(
                Date: date,
                DayOfWeek: date.ToString("dddd"),
                Completed: dayData != null,
                SetsCompleted: dayData?.SetsCompleted ?? 0
            ));
        }

        return weeklyWorkouts;
    }

    private async Task<List<PersonalRecordResponse>> GetRecentPersonalRecords(Guid userId, CancellationToken cancellationToken)
    {
        // Find max load for each exercise/reps combination using database-level grouping
        var prs = await _context.WorkoutSets
            .AsNoTracking()
            .Where(s => s.WorkoutSession.OwnerId == userId &&
                       s.WorkoutSession.CompletedAt != null &&
                       s.Reps.HasValue && s.Load.HasValue)
            .GroupBy(s => new { s.ExerciseId, s.Reps })
            .Select(group => new
            {
                ExerciseId = group.Key.ExerciseId,
                Reps = group.Key.Reps!.Value,
                MaxLoad = group.Max(s => s.Load!.Value),
                // Get the most recent set with this max load
                MostRecentDate = group
                    .Where(s => s.Load == group.Max(x => x.Load))
                    .Max(s => s.WorkoutSession.CompletedAt!.Value)
            })
            .OrderByDescending(pr => pr.MostRecentDate)
            .Take(5)
            .ToListAsync(cancellationToken);

        // Get exercise names for the PRs
        var exerciseIds = prs.Select(pr => pr.ExerciseId).ToList();
        var exercises = await _context.Exercises
            .AsNoTracking()
            .Where(e => exerciseIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Name, cancellationToken);

        return prs.Select(pr => new PersonalRecordResponse(
            pr.ExerciseId,
            exercises.GetValueOrDefault(pr.ExerciseId, "Unknown"),
            pr.Reps,
            pr.MaxLoad,
            pr.MostRecentDate
        )).ToList();
    }
}
