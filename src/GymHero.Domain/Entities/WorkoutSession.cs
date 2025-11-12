namespace GymHero.Domain.Entities;

public class WorkoutSession : BaseEntity
{
    // Owner of this workout session
    public Guid OwnerId { get; set; }

    // Chave estrangeira para o plano de treino que originou esta sessão (nullable para treinos livres)
    public Guid? WorkoutPlanId { get; set; }
    public WorkoutPlan? WorkoutPlan { get; set; }

    // Chave estrangeira para o treino específico dentro do plano (nullable para treinos livres)
    public Guid? WorkoutId { get; set; }
    public Workout? Workout { get; set; }

     public DateTime StartedAt { get; set; } // Renomeado de 'Date'
    public DateTime? CompletedAt { get; set; } // Data de finalização, pode ser nula
    public string? Notes { get; set; }

    // Relação: Uma sessão de treino é composta por várias séries executadas
    public ICollection<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
}