namespace GymHero.Domain.Entities;

public class WorkoutExercise : BaseEntity
{
    // Chave estrangeira para o treino (workout day)
    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;

    // Chave estrangeira para o exercício
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int Order { get; set; } // Ordem do exercício no treino (1, 2, 3...)
    public int TargetSets { get; set; }
    public int TargetReps { get; set; }
    public double TargetLoad { get; set; } // Carga/peso alvo
    public string? TargetRepsRange { get; set; } // Ex: "8-12", "AMRAP"
    public int? TargetRpe { get; set; } // Rate of Perceived Exertion (1-10)
    public int? RestSeconds { get; set; } // Tempo de descanso entre séries
    public string? Notes { get; set; } // Notas sobre o exercício

    // BACKWARD COMPATIBILITY: Propriedades computadas para código legado
    public Guid WorkoutPlanId => Workout?.PlanId ?? Guid.Empty;
    public WorkoutPlan? WorkoutPlan => Workout?.Plan;
}