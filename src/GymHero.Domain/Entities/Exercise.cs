using GymHero.Shared.Enums;

namespace GymHero.Domain.Entities;

public class Exercise : BaseEntity
{
    // Informações Básicas
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Classificação Muscular
    public MuscleGroup MuscleGroup { get; set; }
    public List<MuscleGroup>? SecondaryMuscles { get; set; }

    // Equipamento e Categoria
    public Equipment Equipment { get; set; }
    public ExerciseCategory Category { get; set; }
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Beginner;

    // Instruções e Dicas
    public List<string>? Instructions { get; set; }
    public List<string>? Tips { get; set; }
    public List<string>? CommonMistakes { get; set; }
    public string? Notes { get; set; }

    // Mídia
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }

    // Localização do Treino
    public WorkoutLocation WorkoutLocation { get; set; } = WorkoutLocation.Both;
    public string? SpaceRequired { get; set; } // "Minimal", "Small", "Medium", "Large"

    // Progressões e Regressões (para exercícios de calistenia)
    public List<string>? Progressions { get; set; } // Versões mais difíceis
    public List<string>? Regressions { get; set; } // Versões mais fáceis
    public string? NoEquipmentAlternative { get; set; } // Alternativa sem equipamento

    // Controle de Acesso (para exercícios customizados de PTs)
    public bool IsPublic { get; set; } = true;
    public Guid? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Relação: Um exercício pode estar em muitos "itens de plano de treino"
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}
