namespace GymHero.Domain.Entities;

public class Exercise : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string MuscleGroup { get; set; } = string.Empty;
    public string? Equipment { get; set; }
    public string? Notes { get; set; }
    public string? Category { get; set; }

    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }

    // Relação: Um exercício pode estar em muitos "itens de plano de treino"
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}