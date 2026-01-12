using GymHero.Shared.Enums;

namespace GymHero.Shared.DTOs;

/// <summary>
/// Request to generate a periodized workout plan
/// </summary>
public record GeneratePeriodizedPlanRequest(
    string PlanName,
    string? Description,
    Guid StudentId,
    int DurationWeeks,
    PeriodizationModel Model,
    TrainingPhase StartingPhase,
    int WorkoutsPerWeek,
    List<string> TargetMuscleGroups,
    string? FocusArea = null,  // "Upper", "Lower", "FullBody"
    bool IncludeDeloadWeeks = true,
    ProgressionStrategy ProgressionStrategy = ProgressionStrategy.Linear
);

/// <summary>
/// Configuration for a training phase within periodization
/// </summary>
public record TrainingPhaseConfig(
    TrainingPhase Phase,
    int Weeks,
    int SetsPerExercise,
    string RepsRange,  // e.g., "8-12", "3-5"
    int RestSeconds,
    double IntensityPercent,  // % of 1RM or effort level
    string Notes
);

/// <summary>
/// Response after generating a periodized plan
/// </summary>
public record PeriodizedPlanResponse(
    Guid PlanId,
    string PlanName,
    int TotalWeeks,
    List<PhaseSchedule> PhaseSchedule,
    DateTime StartDate,
    DateTime? EndDate,
    string Summary
);

/// <summary>
/// Schedule of phases in the periodized plan
/// </summary>
public record PhaseSchedule(
    TrainingPhase Phase,
    int StartWeek,
    int EndWeek,
    int WorkoutsCount,
    string Description
);

/// <summary>
/// Template for periodization that can be reused
/// </summary>
public record PeriodizationTemplateDto(
    Guid Id,
    string Name,
    string Description,
    PeriodizationModel Model,
    int RecommendedWeeks,
    List<TrainingPhaseConfig> Phases,
    DateTime CreatedAt
);

/// <summary>
/// Request to create a periodization template
/// </summary>
public record CreatePeriodizationTemplateRequest(
    string Name,
    string Description,
    PeriodizationModel Model,
    int RecommendedWeeks,
    List<TrainingPhaseConfig> Phases
);

/// <summary>
/// Weekly progression details
/// </summary>
public record WeeklyProgression(
    int WeekNumber,
    TrainingPhase Phase,
    double IntensityMultiplier,
    int SetsPerExercise,
    string RepsRange,
    bool IsDeloadWeek
);
