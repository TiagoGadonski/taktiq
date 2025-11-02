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
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        var accountCreatedAt = user?.CreatedAt ?? DateTime.UtcNow;

        // Get all completed workout sessions for the user
        var completedSessions = await _context.WorkoutSessions
            .Include(s => s.Sets)
            .Include(s => s.WorkoutPlan)
            .Where(s => s.CompletedAt != null)
            .Where(s => s.OwnerId == request.UserId)
            .OrderByDescending(s => s.CompletedAt)
            .ToListAsync(cancellationToken);

        // Calculate total workouts (unique days with workouts)
        var totalWorkouts = completedSessions
            .Where(s => s.CompletedAt.HasValue)
            .Select(s => s.CompletedAt!.Value.Date)
            .Distinct()
            .Count();

        // Calculate total unique exercises done
        var totalExercises = completedSessions
            .SelectMany(s => s.Sets)
            .Select(set => set.ExerciseId)
            .Distinct()
            .Count();

        // Calculate total volume (sum of all weight × reps)
        var totalVolume = completedSessions
            .SelectMany(s => s.Sets)
            .Where(set => set.Load.HasValue && set.Reps.HasValue)
            .Sum(set => set.Load!.Value * set.Reps!.Value);

        // Calculate current streak (consecutive days with workouts)
        var currentStreak = CalculateStreak(completedSessions);

        // Get last 7 days of workout activity
        var weeklyWorkouts = GetWeeklyWorkouts(completedSessions);

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

    private int CalculateStreak(List<Domain.Entities.WorkoutSession> sessions)
    {
        if (!sessions.Any())
            return 0;

        var streak = 0;
        var currentDate = DateTime.UtcNow.Date;

        // Get unique workout dates
        var workoutDates = sessions
            .Where(s => s.CompletedAt.HasValue)
            .Select(s => s.CompletedAt!.Value.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

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

    private List<WeeklyWorkoutDto> GetWeeklyWorkouts(List<Domain.Entities.WorkoutSession> sessions)
    {
        var today = DateTime.UtcNow.Date;
        var weeklyWorkouts = new List<WeeklyWorkoutDto>();

        // Get last 7 days
        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            var sessionsOnDate = sessions.Where(s =>
                s.CompletedAt.HasValue &&
                s.CompletedAt.Value.Date == date
            ).ToList();

            var setsCompleted = sessionsOnDate.SelectMany(s => s.Sets).Count();

            weeklyWorkouts.Add(new WeeklyWorkoutDto(
                Date: date,
                DayOfWeek: date.ToString("dddd"),
                Completed: sessionsOnDate.Any(),
                SetsCompleted: setsCompleted
            ));
        }

        return weeklyWorkouts;
    }

    private async Task<List<PersonalRecordResponse>> GetRecentPersonalRecords(Guid userId, CancellationToken cancellationToken)
    {
        // Get all sets with their exercises and sessions
        var allSets = await _context.WorkoutSets
            .Include(s => s.Exercise)
            .Include(s => s.WorkoutSession)
            .Where(s => s.WorkoutSession.OwnerId == userId &&
                       s.WorkoutSession.CompletedAt != null &&
                       s.Reps.HasValue && s.Load.HasValue)
            .ToListAsync(cancellationToken);

        // Group and find max load for each exercise/reps combination in memory
        var prs = allSets
            .GroupBy(s => new { s.ExerciseId, s.Reps })
            .Select(group => group.OrderByDescending(s => s.Load).First())
            .OrderByDescending(s => s.WorkoutSession.CompletedAt)
            .Take(5)
            .Select(s => new PersonalRecordResponse(
                s.ExerciseId,
                s.Exercise.Name,
                s.Reps!.Value,
                s.Load!.Value,
                s.WorkoutSession.CompletedAt!.Value
            ))
            .ToList();

        return prs;
    }
}
