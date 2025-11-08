namespace GymHero.Domain.Entities;

public class WorkoutSet : BaseEntity
{
    // Chave estrangeira para a sessão de treino
    public Guid WorkoutSessionId { get; set; }
    public WorkoutSession WorkoutSession { get; set; } = null!;

    // Chave estrangeira para o exercício realizado
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int SetNumber { get; set; }
    public int? Reps { get; set; } // Repetições realizadas (opcional)
    public double? Load { get; set; } // Carga/peso utilizado (opcional)

    // Rating of Perceived Exertion (Escala de Esforço Percebido)
    public int? Rpe { get; set; }
    public bool Completed { get; set; }

    // Indicates if this exercise was added during the session (not part of the original workout plan)
    public bool IsAddedDuringSession { get; set; } = false;
}