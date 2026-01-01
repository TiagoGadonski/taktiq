namespace GymHero.Domain.Entities;

public class WorkoutSessionFeedback : BaseEntity
{
    // Relacionamento com a sessão de treino
    public Guid SessionId { get; set; }
    public WorkoutSession Session { get; set; } = null!;

    // Relacionamento com o usuário (aluno)
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // ===== AVALIAÇÕES (escala 1-5) =====
    public int DifficultyRating { get; set; }      // 1 = Muito fácil, 5 = Muito difícil
    public int EnergyLevel { get; set; }           // 1 = Exausto, 5 = Energizado
    public int OverallSatisfaction { get; set; }   // 1 = Péssimo, 5 = Excelente

    // ===== ÁREAS COM DOR/DESCONFORTO =====
    // CSV: "lower back,knee,shoulder"
    public string? PainAreas { get; set; }

    // ===== EXERCÍCIOS FAVORITOS/NÃO GOSTOU =====
    // CSV de nomes de exercícios
    public string? FavoriteExercises { get; set; }
    public string? DislikedExercises { get; set; }

    // ===== COMENTÁRIOS LIVRES =====
    public string? Comments { get; set; }

    // ===== TIMESTAMP =====
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
