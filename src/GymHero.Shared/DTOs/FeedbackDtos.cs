namespace GymHero.Shared.DTOs;

// Request para criar feedback após treino
public record SubmitFeedbackRequest(
    int DifficultyRating,      // 1-5
    int EnergyLevel,           // 1-5
    int OverallSatisfaction,   // 1-5
    List<string>? PainAreas,   // ["lower back", "knee", "shoulder"]
    List<string>? FavoriteExercises,  // Nomes dos exercícios que gostou
    List<string>? DislikedExercises,  // Nomes dos exercícios que não gostou
    string? Comments           // Comentários livres
);

// Response com dados do feedback
public record FeedbackResponse(
    Guid Id,
    Guid SessionId,
    Guid UserId,
    int DifficultyRating,
    int EnergyLevel,
    int OverallSatisfaction,
    List<string>? PainAreas,
    List<string>? FavoriteExercises,
    List<string>? DislikedExercises,
    string? Comments,
    DateTime SubmittedAt
);

// Estatísticas do aluno para o PT
public record StudentStatsResponse(
    Guid StudentId,
    string StudentName,
    DateTime PeriodStart,
    DateTime PeriodEnd,

    // Métricas de frequência
    int TotalWorkoutsScheduled,
    int CompletedWorkouts,
    double CompletionRate,  // Percentual 0-100

    // Métricas de feedback
    double? AverageDifficulty,      // 1-5
    double? AverageEnergy,          // 1-5
    double? AverageSatisfaction,    // 1-5

    // Áreas problemáticas
    List<PainAreaFrequency> FrequentPainAreas,

    // Frequência por dia da semana
    List<WorkoutFrequencyByDay> FrequencyByDay,

    // Feedback recente
    List<RecentFeedbackSummary> RecentFeedback
);

public record PainAreaFrequency(
    string Area,
    int Count,
    double Percentage  // % das sessões em que apareceu
);

public record WorkoutFrequencyByDay(
    string DayOfWeek,      // "Monday", "Tuesday", etc.
    int Count,
    double Percentage      // % do total
);

public record RecentFeedbackSummary(
    DateTime Date,
    string WorkoutName,
    int Difficulty,
    int Satisfaction,
    string? Comments
);
