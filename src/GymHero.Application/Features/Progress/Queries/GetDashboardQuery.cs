using GymHero.Application.Common.Behaviors;
using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Progress.Queries;

public record GetDashboardQuery(Guid UserId) : IRequest<DashboardResponse>, ICacheableQuery
{
    public string CacheKey => $"Dashboard_{UserId}";

    // Cache for 2 minutes - balance between freshness and performance
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(2);
}

public record DashboardResponse(
    int TotalWorkouts,
    int TotalSets,
    double TotalVolume,
    int CurrentStreak,
    List<WeeklyWorkoutDto> WeeklyWorkouts,
    List<PersonalRecordResponse> RecentPRs,
    DateTime AccountCreatedAt
);

public record WeeklyWorkoutDto(
    DateTime Date,
    string DayOfWeek,
    bool Completed,
    int SetsCompleted
);
