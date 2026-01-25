namespace GymHero.Shared.DTOs;

// ==========================================
// SIMPLIFIED AI WORKOUT GENERATION DTOs
// ==========================================

/// <summary>
/// Request para gerar um treino unico (rapido, 4 campos obrigatorios)
/// </summary>
public record QuickWorkoutRequest(
    string Goal,              // Hipertrofia, Emagrecimento, Forca, Condicionamento
    string Level,             // Iniciante, Intermediario, Avancado
    string Location,          // Academia, Casa
    List<string> MuscleGroups, // Peito, Costas, Pernas, etc. ou "Corpo Todo"
    int? Duration = null,      // Opcional: 30, 45, 60, 90 minutos
    string? Injuries = null,   // Opcional: texto livre
    string? Notes = null       // Opcional: texto livre
);

/// <summary>
/// Request para gerar um plano semanal (5 campos obrigatorios)
/// </summary>
public record WeeklyPlanRequest(
    string Goal,               // Hipertrofia, Emagrecimento, Forca, Condicionamento
    string Level,              // Iniciante, Intermediario, Avancado
    string Location,           // Academia, Casa
    int DaysPerWeek,           // 2-6
    int Weeks,                 // 4, 8, 12
    string? SplitType = null,  // Opcional: Full Body, Upper/Lower, Push/Pull/Legs, ABC
    List<string>? PriorityMuscles = null, // Opcional
    string? Injuries = null,
    string? Notes = null
);

/// <summary>
/// Exercicio gerado pela IA com ID do banco
/// </summary>
public record GeneratedExercise(
    Guid ExerciseId,
    string ExerciseName,
    string MuscleGroup,
    string Equipment,
    int Sets,
    string Reps,
    int RestSeconds,
    string? VideoUrl = null,
    string? ImageUrl = null,
    string? Notes = null,
    string? ExerciseType = null  // warmup, main, cooldown
);

/// <summary>
/// Response do treino gerado
/// </summary>
public record QuickWorkoutResponse(
    string Name,
    string Description,
    int EstimatedDuration,
    string Goal,
    string Level,
    List<GeneratedExercise> Warmup,
    List<GeneratedExercise> Main,
    List<GeneratedExercise> Cooldown
);

/// <summary>
/// Um dia de treino no plano semanal
/// </summary>
public record PlanWorkoutDay(
    int DayNumber,
    string DayName,
    string Focus,
    List<GeneratedExercise> Exercises
);

/// <summary>
/// Uma semana do plano
/// </summary>
public record PlanWeek(
    int WeekNumber,
    string Focus,  // Adaptacao, Volume, Intensidade, Deload
    List<PlanWorkoutDay> Workouts
);

/// <summary>
/// Response do plano semanal gerado
/// </summary>
public record WeeklyPlanResponse(
    string Name,
    string Description,
    string Goal,
    string Level,
    string SplitType,
    int WeeksCount,
    int DaysPerWeek,
    List<PlanWeek> Weeks,
    string ProgressionNotes
);
