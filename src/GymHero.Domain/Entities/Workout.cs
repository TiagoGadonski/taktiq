namespace GymHero.Domain.Entities;

public class Workout : BaseEntity
{
    // Chave estrangeira para o plano de treino
    public Guid PlanId { get; set; }
    public WorkoutPlan Plan { get; set; } = null!;

    public string Name { get; set; } = string.Empty; // Ex: "Treino A - Peito e Tríceps"
    public int? DayOfWeek { get; set; } // 0-6 (Domingo-Sábado), null se não for um dia específico
    public int Order { get; set; } // Ordem dentro do plano (1, 2, 3...)

    // Um treino tem uma coleção de exercícios
    public ICollection<WorkoutExercise> Exercises { get; set; } = new List<WorkoutExercise>();
}
