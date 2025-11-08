using GymHero.Domain.Enums;

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
    public DateTime? StartDate { get; set; } // Data de início do plano
    public DateTime? ExpirationDate { get; set; } // Data de expiração calculada com base na duração

    // Sharing and visibility settings
    public VisibilityLevel VisibilityLevel { get; set; } = VisibilityLevel.Private;
    public bool IsPublic { get; set; } = false; // Shortcut for VisibilityLevel == Public
    public DateTime? PublishedAt { get; set; }
    public bool AllowCopying { get; set; } = true; // Allow others to copy this plan
    public int ViewCount { get; set; } = 0; // Track how many times plan was viewed

    // Um plano de treino tem uma coleção de treinos (workout days).
    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
    public ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
}