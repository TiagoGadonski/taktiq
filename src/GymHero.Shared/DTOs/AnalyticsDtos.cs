namespace GymHero.Shared.DTOs;

// Weekly workout activity DTO
public record WeeklyWorkoutDto(
    DateTime Date,
    string DayOfWeek,
    bool Completed,
    int SetsCompleted
);

// Trainer's comprehensive dashboard
public record TrainerDashboardResponse(
    // Overview
    int TotalStudents,
    int ActiveStudents,  // Students with workouts in last 30 days
    int InactiveStudents,  // Students with no workouts in last 30 days

    // Student Growth
    int NewStudentsThisMonth,
    int NewStudentsLastMonth,
    double StudentGrowthPercentage,

    // Engagement
    int TotalWorkoutsThisMonth,
    int TotalWorkoutsLastMonth,
    double WorkoutGrowthPercentage,
    double AverageWorkoutsPerStudent,
    double StudentEngagementRate,  // % of students who worked out in last 7 days

    // Assessments & Progress
    int TotalAssessmentsConducted,
    int AssessmentsThisMonth,
    int TotalProgressPhotos,
    int ProgressPhotosThisMonth,

    // Content
    int TotalWorkoutPlans,
    int TotalCustomExercises,
    int TotalBlogPosts,

    // Financial (if applicable)
    decimal? MonthlyRevenue,
    decimal? TotalRevenue,

    // Top Students
    List<TopStudentSummary> MostActiveStudents,
    List<TopStudentSummary> TopPerformers,

    // Recent Activity
    List<RecentActivityItem> RecentActivities,

    // Time-series data
    List<MonthlyMetric> StudentGrowthChart,  // Last 12 months
    List<MonthlyMetric> WorkoutVolumeChart,  // Last 12 months

    DateTime GeneratedAt
);

public record TopStudentSummary(
    Guid StudentId,
    string StudentName,
    string? ProfilePictureUrl,
    int WorkoutsCompleted,
    int CurrentStreak,
    DateTime LastWorkout
);

public record RecentActivityItem(
    string Type,  // "workout", "assessment", "photo", "plan_created", etc.
    string Description,
    Guid? StudentId,
    string? StudentName,
    DateTime Timestamp
);

public record MonthlyMetric(
    int Year,
    int Month,
    string MonthName,
    double Value
);

// Individual student detailed analytics
public record StudentDetailedAnalyticsResponse(
    // Student Info
    Guid StudentId,
    string StudentName,
    string? ProfilePictureUrl,
    DateTime MemberSince,
    int DaysAsMember,

    // Workout Stats
    int TotalWorkouts,
    int TotalSets,
    double TotalVolume,  // kg × reps
    int CurrentStreak,
    DateTime? LastWorkout,
    double AverageWorkoutsPerWeek,
    double AverageSessionDuration,  // minutes

    // Progress Metrics
    List<ProgressMetricResponse> BodyMetrics,
    List<PersonalRecordResponse> PersonalRecords,
    List<ProgressPhotoSummaryResponse>? ProgressPhotos,

    // Assessments
    int TotalAssessments,
    DateTime? LastAssessmentDate,
    List<AssessmentResponse>? RecentAssessments,
    List<ProtocolResultSummaryResponse>? ProtocolResults,

    // Engagement
    double EngagementScore,  // 0-100
    string EngagementLevel,  // "Excellent", "Good", "Fair", "Poor"
    List<WeeklyWorkoutDto> Last4WeeksActivity,

    // Workout Distribution
    List<MuscleGroupDistribution> MuscleGroupDistribution,
    List<ExerciseFrequency> MostPerformedExercises,

    // Goals & Plans
    int ActiveWorkoutPlans,
    DateTime? CurrentPlanStartDate,
    DateTime? CurrentPlanEndDate,

    // Feedback & Satisfaction
    double? AverageDifficultyRating,
    double? AverageEnergyLevel,
    double? AverageSatisfaction,
    List<PainAreaFrequency>? FrequentPainAreas,

    DateTime GeneratedAt
);

public record MuscleGroupDistribution(
    string MuscleGroup,
    int WorkoutsCount,
    double Percentage
);

public record ExerciseFrequency(
    Guid ExerciseId,
    string ExerciseName,
    int TimesPerformed,
    DateTime LastPerformed,
    double? BestLoad
);

// Comparative analytics (multiple students)
public record ComparativeAnalyticsResponse(
    DateTime StartDate,
    DateTime EndDate,
    List<StudentComparisonSummary> Students,
    ComparisonInsights Insights
);

public record StudentComparisonSummary(
    Guid StudentId,
    string StudentName,
    int WorkoutsCompleted,
    double TotalVolume,
    int PersonalRecordsBroken,
    double EngagementScore,
    double ImprovementScore  // Based on various metrics
);

public record ComparisonInsights(
    string TopPerformer,
    string MostImproved,
    string MostConsistent,
    double AverageWorkouts,
    double AverageVolume
);

// Revenue analytics - using existing RevenueAnalyticsResponse from PaymentDtos

// Engagement report
public record EngagementReportResponse(
    DateTime StartDate,
    DateTime EndDate,

    // Overall engagement
    double OverallEngagementRate,
    int ActiveStudents,
    int InactiveStudents,
    int AtRiskStudents,  // No workout in 7-14 days
    int ChurnedStudents,  // No workout in 30+ days

    // Detailed breakdowns
    List<StudentEngagementDetail> StudentDetails,
    List<EngagementByDay> EngagementByDayOfWeek,
    List<EngagementByHour> EngagementByHourOfDay,

    // Retention
    double RetentionRate30Days,
    double RetentionRate90Days,

    DateTime GeneratedAt
);

public record StudentEngagementDetail(
    Guid StudentId,
    string StudentName,
    int WorkoutsInPeriod,
    int DaysSinceLastWorkout,
    string Status,  // "Active", "At Risk", "Inactive", "Churned"
    double EngagementScore
);

public record EngagementByDay(
    string DayOfWeek,
    int WorkoutsCount,
    int UniqueStudents,
    double AverageWorkoutsPerStudent
);

public record EngagementByHour(
    int Hour,
    string TimeRange,
    int WorkoutsCount
);
