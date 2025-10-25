namespace GymHero.Shared.DTOs;

public record StartSessionRequest(Guid? WorkoutPlanId);
public record LogSetRequest(
    Guid ExerciseId,
    int SetNumber,
    int? Reps, // Opcional - usuário pode entrar apenas se quiser
    double? Load, // Opcional - usuário pode entrar apenas se quiser
    int? Rpe // Rating of Perceived Exertion é opcional
);

public class WorkoutSessionDto
{
    public Guid Id { get; set; }
    public Guid? WorkoutPlanId { get; set; }
    public WorkoutPlanDetailResponse? WorkoutPlan { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<WorkoutSetDto> Sets { get; set; } = new();
}

public class WorkoutSetDto
{
    public Guid Id { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = "";
    public int SetNumber { get; set; }
    public int? Reps { get; set; }
    public double? Load { get; set; }
    public int? Rpe { get; set; }
}