using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Progress.Queries;

public record GetDashboardQuery(Guid UserId) : IRequest<DashboardResponse>;

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
