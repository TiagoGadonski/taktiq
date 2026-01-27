using System.Text.Json.Serialization;

namespace GymHero.Shared.DTOs;

// ==========================================
// SIMPLIFIED AI WORKOUT GENERATION DTOs
// ==========================================

/// <summary>
/// Request para gerar um treino unico (rapido, 4 campos obrigatorios)
/// </summary>
public class QuickWorkoutRequest
{
    [JsonPropertyName("goal")]
    public string Goal { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("muscleGroups")]
    public List<string> MuscleGroups { get; set; } = new();

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("injuries")]
    public string? Injuries { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

/// <summary>
/// Request para gerar um plano semanal (5 campos obrigatorios)
/// </summary>
public class WeeklyPlanRequest
{
    [JsonPropertyName("goal")]
    public string Goal { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("daysPerWeek")]
    public int DaysPerWeek { get; set; }

    [JsonPropertyName("weeks")]
    public int Weeks { get; set; }

    [JsonPropertyName("splitType")]
    public string? SplitType { get; set; }

    [JsonPropertyName("priorityMuscles")]
    public List<string>? PriorityMuscles { get; set; }

    [JsonPropertyName("injuries")]
    public string? Injuries { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

/// <summary>
/// Exercicio gerado pela IA com ID do banco
/// </summary>
public record GeneratedExercise(
    [property: JsonPropertyName("exerciseId")] Guid ExerciseId,
    [property: JsonPropertyName("exerciseName")] string ExerciseName,
    [property: JsonPropertyName("muscleGroup")] string MuscleGroup,
    [property: JsonPropertyName("equipment")] string Equipment,
    [property: JsonPropertyName("sets")] int Sets,
    [property: JsonPropertyName("reps")] string Reps,
    [property: JsonPropertyName("restSeconds")] int RestSeconds,
    [property: JsonPropertyName("videoUrl")] string? VideoUrl = null,
    [property: JsonPropertyName("imageUrl")] string? ImageUrl = null,
    [property: JsonPropertyName("notes")] string? Notes = null,
    [property: JsonPropertyName("exerciseType")] string? ExerciseType = null
);

/// <summary>
/// Response do treino gerado
/// </summary>
public record QuickWorkoutResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("estimatedDuration")] int EstimatedDuration,
    [property: JsonPropertyName("goal")] string Goal,
    [property: JsonPropertyName("level")] string Level,
    [property: JsonPropertyName("warmup")] List<GeneratedExercise> Warmup,
    [property: JsonPropertyName("main")] List<GeneratedExercise> Main,
    [property: JsonPropertyName("cooldown")] List<GeneratedExercise> Cooldown
);

/// <summary>
/// Um dia de treino no plano semanal
/// </summary>
public record PlanWorkoutDay(
    [property: JsonPropertyName("dayNumber")] int DayNumber,
    [property: JsonPropertyName("dayName")] string DayName,
    [property: JsonPropertyName("focus")] string Focus,
    [property: JsonPropertyName("exercises")] List<GeneratedExercise> Exercises
);

/// <summary>
/// Uma semana do plano
/// </summary>
public record PlanWeek(
    [property: JsonPropertyName("weekNumber")] int WeekNumber,
    [property: JsonPropertyName("focus")] string Focus,
    [property: JsonPropertyName("workouts")] List<PlanWorkoutDay> Workouts
);

/// <summary>
/// Response do plano semanal gerado
/// </summary>
public record WeeklyPlanResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("goal")] string Goal,
    [property: JsonPropertyName("level")] string Level,
    [property: JsonPropertyName("splitType")] string SplitType,
    [property: JsonPropertyName("weeksCount")] int WeeksCount,
    [property: JsonPropertyName("daysPerWeek")] int DaysPerWeek,
    [property: JsonPropertyName("weeks")] List<PlanWeek> Weeks,
    [property: JsonPropertyName("progressionNotes")] string ProgressionNotes
);
