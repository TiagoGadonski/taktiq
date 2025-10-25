namespace GymHero.Domain.Entities;

public class WorkoutPlan : BaseEntity
{
    // Chave estrangeira para o dono do plano
    public Guid OwnerId { get; set; }

    // Propriedade de navegação para o EF Core entender o relacionamento.
    // O 'null!' nos diz para o compilador: "confie em mim, isso não será nulo quando eu usar".
    public User Owner { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Goal { get; set; } // O '?' indica que a string pode ser nula (opcional).
    public int? Duration { get; set; } // Duração em semanas
    public bool IsActive { get; set; } = false; // Indica se este é o plano ativo do usuário

    // Um plano de treino tem uma coleção de treinos (workout days).
    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
    public ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
}