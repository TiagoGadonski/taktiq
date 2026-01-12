using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics")
            .WithTags("Analytics")
            .RequireAuthorization("RequirePersonalRole");

        // GET /api/analytics/dashboard - Comprehensive trainer dashboard
        group.MapGet("/dashboard", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var now = DateTime.UtcNow;
            var thisMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var last7Days = now.AddDays(-7);
            var last30Days = now.AddDays(-30);

            // Total students
            var allStudents = await context.Users
                .Where(u => u.PersonalTrainerId == trainerId)
                .ToListAsync(cancellationToken);

            var totalStudents = allStudents.Count;

            // Active vs Inactive students (based on last 30 days)
            var studentWorkoutsLast30Days = await context.WorkoutSessions
                .Where(s => s.CompletedAt >= last30Days &&
                           s.Workout != null && s.Workout.Plan != null &&
                           allStudents.Select(st => st.Id).Contains(s.Workout.Plan.OwnerId))
                .Select(s => s.Workout!.Plan!.OwnerId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var activeStudents = studentWorkoutsLast30Days.Count;
            var inactiveStudents = totalStudents - activeStudents;

            // Student growth
            var newStudentsThisMonth = allStudents.Count(s => s.CreatedAt >= thisMonthStart);
            var newStudentsLastMonth = allStudents.Count(s => s.CreatedAt >= lastMonthStart && s.CreatedAt < thisMonthStart);
            var studentGrowth = newStudentsLastMonth > 0
                ? ((double)(newStudentsThisMonth - newStudentsLastMonth) / newStudentsLastMonth) * 100
                : 0;

            // Workouts this month vs last month
            var workoutsThisMonth = await context.WorkoutSessions
                .Where(s => s.CompletedAt >= thisMonthStart &&
                           s.Workout != null && s.Workout.Plan != null &&
                           allStudents.Select(st => st.Id).Contains(s.Workout.Plan.OwnerId))
                .CountAsync(cancellationToken);

            var workoutsLastMonth = await context.WorkoutSessions
                .Where(s => s.CompletedAt >= lastMonthStart && s.CompletedAt < thisMonthStart &&
                           s.Workout != null && s.Workout.Plan != null &&
                           allStudents.Select(st => st.Id).Contains(s.Workout.Plan.OwnerId))
                .CountAsync(cancellationToken);

            var workoutGrowth = workoutsLastMonth > 0
                ? ((double)(workoutsThisMonth - workoutsLastMonth) / workoutsLastMonth) * 100
                : 0;

            var avgWorkoutsPerStudent = totalStudents > 0 ? (double)workoutsThisMonth / totalStudents : 0;

            // Engagement rate (% of students who worked out in last 7 days)
            var studentWorkoutsLast7Days = await context.WorkoutSessions
                .Where(s => s.CompletedAt >= last7Days &&
                           s.Workout != null && s.Workout.Plan != null &&
                           allStudents.Select(st => st.Id).Contains(s.Workout.Plan.OwnerId))
                .Select(s => s.Workout!.Plan!.OwnerId)
                .Distinct()
                .CountAsync(cancellationToken);

            var engagementRate = totalStudents > 0 ? ((double)studentWorkoutsLast7Days / totalStudents) * 100 : 0;

            // Assessments
            var totalAssessments = await context.StudentAssessments
                .Where(a => a.TrainerId == trainerId)
                .CountAsync(cancellationToken);

            var assessmentsThisMonth = await context.StudentAssessments
                .Where(a => a.TrainerId == trainerId && a.AssessmentDate >= thisMonthStart)
                .CountAsync(cancellationToken);

            // Progress photos
            var totalPhotos = await context.ProgressPhotos
                .Where(p => p.UploadedBy == trainerId && !p.IsDeleted)
                .CountAsync(cancellationToken);

            var photosThisMonth = await context.ProgressPhotos
                .Where(p => p.UploadedBy == trainerId && !p.IsDeleted && p.CreatedAt >= thisMonthStart)
                .CountAsync(cancellationToken);

            // Content
            var totalPlans = await context.WorkoutPlans
                .Where(p => p.OwnerId == trainerId)
                .CountAsync(cancellationToken);

            var totalCustomExercises = await context.Exercises
                .Where(e => e.CreatedByUserId == trainerId)
                .CountAsync(cancellationToken);

            var totalPosts = await context.Posts
                .Where(p => p.AuthorId == trainerId)
                .CountAsync(cancellationToken);

            // Revenue (if transactions exist)
            var monthlyRevenue = await context.Transactions
                .Where(t => t.SellerId == trainerId &&
                           t.Status == TransactionStatus.Completed &&
                           t.CreatedAt >= thisMonthStart)
                .SumAsync(t => t.Amount, cancellationToken);

            var totalRevenue = await context.Transactions
                .Where(t => t.SellerId == trainerId && t.Status == TransactionStatus.Completed)
                .SumAsync(t => t.Amount, cancellationToken);

            // Top students (most workouts this month)
            var topActiveStudents = await context.WorkoutSessions
                .Where(s => s.CompletedAt >= thisMonthStart &&
                           s.Workout != null && s.Workout.Plan != null &&
                           allStudents.Select(st => st.Id).Contains(s.Workout.Plan.OwnerId))
                .GroupBy(s => s.Workout!.Plan!.OwnerId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    WorkoutCount = g.Count(),
                    LastWorkout = g.Max(s => s.CompletedAt!.Value)
                })
                .OrderByDescending(x => x.WorkoutCount)
                .Take(5)
                .ToListAsync(cancellationToken);

            var topStudentIds = topActiveStudents.Select(s => s.StudentId).ToList();
            var topStudentDetails = await context.Users
                .Where(u => topStudentIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, cancellationToken);

            // Calculate streaks for top students (simplified)
            var topStudents = topActiveStudents.Select(s => new TopStudentSummary(
                s.StudentId,
                topStudentDetails[s.StudentId].Name,
                topStudentDetails[s.StudentId].ProfilePictureUrl,
                s.WorkoutCount,
                0,  // Simplified - streak calculation would require more complex logic
                s.LastWorkout
            )).ToList();

            // Recent activities
            var recentWorkouts = await context.WorkoutSessions
                .Include(s => s.Workout)
                    .ThenInclude(w => w!.Plan)
                .Where(s => s.CompletedAt.HasValue &&
                           s.Workout != null && s.Workout.Plan != null &&
                           allStudents.Select(st => st.Id).Contains(s.Workout.Plan.OwnerId))
                .OrderByDescending(s => s.CompletedAt)
                .Take(10)
                .Select(s => new RecentActivityItem(
                    "workout",
                    $"Completou treino: {s.Workout!.Name}",
                    s.Workout.Plan!.OwnerId,
                    topStudentDetails.ContainsKey(s.Workout.Plan.OwnerId)
                        ? topStudentDetails[s.Workout.Plan.OwnerId].Name
                        : "Unknown",
                    s.CompletedAt!.Value
                ))
                .ToListAsync(cancellationToken);

            // Student growth chart (last 12 months)
            var studentGrowthChart = new List<MonthlyMetric>();
            for (int i = 11; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var count = allStudents.Count(s => s.CreatedAt < monthEnd);

                studentGrowthChart.Add(new MonthlyMetric(
                    month.Year,
                    month.Month,
                    month.ToString("MMM"),
                    count
                ));
            }

            // Workout volume chart (last 12 months)
            var workoutVolumeChart = new List<MonthlyMetric>();
            for (int i = 11; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1);

                var count = await context.WorkoutSessions
                    .Where(s => s.CompletedAt >= monthStart && s.CompletedAt < monthEnd &&
                               s.Workout != null && s.Workout.Plan != null &&
                               allStudents.Select(st => st.Id).Contains(s.Workout.Plan.OwnerId))
                    .CountAsync(cancellationToken);

                workoutVolumeChart.Add(new MonthlyMetric(
                    month.Year,
                    month.Month,
                    month.ToString("MMM"),
                    count
                ));
            }

            var dashboard = new TrainerDashboardResponse(
                totalStudents,
                activeStudents,
                inactiveStudents,
                newStudentsThisMonth,
                newStudentsLastMonth,
                Math.Round(studentGrowth, 1),
                workoutsThisMonth,
                workoutsLastMonth,
                Math.Round(workoutGrowth, 1),
                Math.Round(avgWorkoutsPerStudent, 1),
                Math.Round(engagementRate, 1),
                totalAssessments,
                assessmentsThisMonth,
                totalPhotos,
                photosThisMonth,
                totalPlans,
                totalCustomExercises,
                totalPosts,
                monthlyRevenue,
                totalRevenue,
                topStudents,
                new List<TopStudentSummary>(),  // Top performers - would need more complex calculation
                recentWorkouts,
                studentGrowthChart,
                workoutVolumeChart,
                now
            );

            return Results.Ok(dashboard);
        })
        .WithName("GetTrainerDashboard")
        .WithSummary("Get comprehensive analytics dashboard for trainer");

        // GET /api/analytics/student/{studentId} - Detailed student analytics
        group.MapGet("/student/{studentId:guid}", async (
            Guid studentId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Verify student belongs to trainer
            var student = await context.Users
                .FirstOrDefaultAsync(u => u.Id == studentId && u.PersonalTrainerId == trainerId, cancellationToken);

            if (student == null)
            {
                return Results.Problem(
                    title: "Acesso negado",
                    detail: "Este aluno não pertence a você",
                    statusCode: 403
                );
            }

            var now = DateTime.UtcNow;
            var daysAsMember = (now - student.CreatedAt).Days;

            // Workout stats
            var completedSessions = await context.WorkoutSessions
                .Where(s => s.Workout!.Plan!.OwnerId == studentId && s.CompletedAt.HasValue)
                .ToListAsync(cancellationToken);

            var totalWorkouts = completedSessions.Select(s => s.CompletedAt!.Value.Date).Distinct().Count();

            var totalSets = await context.WorkoutSets
                .Where(s => s.WorkoutSession.Workout!.Plan!.OwnerId == studentId &&
                           s.WorkoutSession.CompletedAt.HasValue)
                .CountAsync(cancellationToken);

            var totalVolume = await context.WorkoutSets
                .Where(s => s.WorkoutSession.Workout!.Plan!.OwnerId == studentId &&
                           s.WorkoutSession.CompletedAt.HasValue &&
                           s.Load.HasValue && s.Reps.HasValue)
                .SumAsync(s => s.Load!.Value * s.Reps!.Value, cancellationToken);

            var lastWorkout = completedSessions.Any() ? completedSessions.Max(s => s.CompletedAt) : null;

            var weeksAsMember = daysAsMember / 7.0;
            var avgWorkoutsPerWeek = weeksAsMember > 0 ? totalWorkouts / weeksAsMember : 0;

            // Progress metrics
            var bodyMetrics = await context.ProgressMetrics
                .Where(m => m.OwnerId == studentId)
                .OrderByDescending(m => m.Date)
                .Select(m => new ProgressMetricResponse(m.Id, m.Type, m.Value, m.Unit, m.Date))
                .ToListAsync(cancellationToken);

            // Personal records
            var personalRecords = await context.WorkoutSets
                .Where(s => s.WorkoutSession.Workout!.Plan!.OwnerId == studentId &&
                           s.WorkoutSession.CompletedAt.HasValue &&
                           s.Reps.HasValue && s.Load.HasValue)
                .Include(s => s.Exercise)
                .Include(s => s.WorkoutSession)
                .GroupBy(s => new { s.ExerciseId, s.Reps })
                .Select(g => g.OrderByDescending(s => s.Load).First())
                .Select(s => new PersonalRecordResponse(
                    s.ExerciseId,
                    s.Exercise.Name,
                    s.Reps!.Value,
                    s.Load!.Value,
                    s.WorkoutSession.CompletedAt!.Value))
                .ToListAsync(cancellationToken);

            // Assessments
            var totalAssessments = await context.StudentAssessments
                .Where(a => a.StudentId == studentId)
                .CountAsync(cancellationToken);

            var lastAssessment = await context.StudentAssessments
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.AssessmentDate)
                .FirstOrDefaultAsync(cancellationToken);

            // Engagement calculation
            var last30Days = now.AddDays(-30);
            var workoutsLast30Days = completedSessions.Count(s => s.CompletedAt >= last30Days);
            var engagementScore = Math.Min(100, (workoutsLast30Days / 20.0) * 100);  // 20 workouts = 100%

            var engagementLevel = engagementScore switch
            {
                >= 80 => "Excellent",
                >= 60 => "Good",
                >= 40 => "Fair",
                _ => "Poor"
            };

            // Last 4 weeks activity
            var last4Weeks = new List<WeeklyWorkoutDto>();
            for (int i = 27; i >= 0; i -= 7)
            {
                var weekStart = now.AddDays(-i).Date;
                var weekEnd = weekStart.AddDays(7);

                var weekWorkouts = completedSessions.Count(s =>
                    s.CompletedAt >= weekStart && s.CompletedAt < weekEnd);

                last4Weeks.Add(new WeeklyWorkoutDto(
                    weekStart,
                    weekStart.ToString("dddd"),
                    weekWorkouts > 0,
                    weekWorkouts
                ));
            }

            var analytics = new StudentDetailedAnalyticsResponse(
                studentId,
                student.Name,
                student.ProfilePictureUrl,
                student.CreatedAt,
                daysAsMember,
                totalWorkouts,
                totalSets,
                totalVolume,
                0,  // Streak calculation simplified
                lastWorkout,
                Math.Round(avgWorkoutsPerWeek, 1),
                0,  // Average duration - would need session time tracking
                bodyMetrics,
                personalRecords,
                null,  // Progress photos - could be added
                totalAssessments,
                lastAssessment?.AssessmentDate,
                null,  // Recent assessments - could be added
                null,  // Protocol results - could be added
                Math.Round(engagementScore, 1),
                engagementLevel,
                last4Weeks,
                new List<MuscleGroupDistribution>(),  // Would need muscle group tracking
                new List<ExerciseFrequency>(),  // Would need exercise frequency tracking
                0,  // Active plans
                null,
                null,
                null,  // Average ratings
                null,
                null,
                null,  // Pain areas
                now
            );

            return Results.Ok(analytics);
        })
        .WithName("GetStudentDetailedAnalytics")
        .WithSummary("Get detailed analytics for a specific student");

        // GET /api/analytics/engagement - Engagement report
        group.MapGet("/engagement", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken cancellationToken) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var now = DateTime.UtcNow;
            var start = startDate ?? now.AddDays(-30);
            var end = endDate ?? now;

            var allStudents = await context.Users
                .Where(u => u.PersonalTrainerId == trainerId)
                .ToListAsync(cancellationToken);

            var last7Days = now.AddDays(-7);
            var last14Days = now.AddDays(-14);
            var last30Days = now.AddDays(-30);

            // Get all workout dates for students
            var studentWorkouts = await context.WorkoutSessions
                .Where(s => s.CompletedAt.HasValue &&
                           s.Workout != null && s.Workout.Plan != null &&
                           allStudents.Select(st => st.Id).Contains(s.Workout.Plan.OwnerId))
                .GroupBy(s => s.Workout!.Plan!.OwnerId)
                .Select(g => new
                {
                    StudentId = g.Key,
                    LastWorkout = g.Max(s => s.CompletedAt!.Value),
                    WorkoutsInPeriod = g.Count(s => s.CompletedAt >= start && s.CompletedAt <= end)
                })
                .ToListAsync(cancellationToken);

            var activeStudents = studentWorkouts.Count(s => s.LastWorkout >= last7Days);
            var atRiskStudents = studentWorkouts.Count(s => s.LastWorkout >= last14Days && s.LastWorkout < last7Days);
            var inactiveStudents = studentWorkouts.Count(s => s.LastWorkout >= last30Days && s.LastWorkout < last14Days);
            var churnedStudents = allStudents.Count - studentWorkouts.Count(s => s.LastWorkout >= last30Days);

            var overallEngagement = allStudents.Count > 0
                ? ((double)activeStudents / allStudents.Count) * 100
                : 0;

            // Student details
            var studentDetails = studentWorkouts.Select(s =>
            {
                var daysSinceLastWorkout = (now - s.LastWorkout).Days;
                var status = daysSinceLastWorkout switch
                {
                    <= 7 => "Active",
                    <= 14 => "At Risk",
                    <= 30 => "Inactive",
                    _ => "Churned"
                };

                var score = Math.Max(0, 100 - (daysSinceLastWorkout * 3));

                var student = allStudents.First(st => st.Id == s.StudentId);

                return new StudentEngagementDetail(
                    s.StudentId,
                    student.Name,
                    s.WorkoutsInPeriod,
                    daysSinceLastWorkout,
                    status,
                    score
                );
            }).OrderByDescending(s => s.EngagementScore).ToList();

            var report = new EngagementReportResponse(
                start,
                end,
                Math.Round(overallEngagement, 1),
                activeStudents,
                inactiveStudents,
                atRiskStudents,
                churnedStudents,
                studentDetails,
                new List<EngagementByDay>(),  // Would need day-of-week breakdown
                new List<EngagementByHour>(),  // Would need hour-of-day breakdown
                0,  // Retention rates would need cohort analysis
                0,
                now
            );

            return Results.Ok(report);
        })
        .WithName("GetEngagementReport")
        .WithSummary("Get engagement report for all students");
    }
}
