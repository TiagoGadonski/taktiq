using GymHero.Domain.Enums;

namespace GymHero.Domain.Entities;

public class Exercise : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MuscleGroup { get; set; } = string.Empty;
    public string? Equipment { get; set; }
    public string? Notes { get; set; }
    public string? Category { get; set; }

    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }

    // Workout Location: Gym, Home, or Both
    public WorkoutLocation WorkoutLocation { get; set; } = WorkoutLocation.Both;

    // Relação: Um exercício pode estar em muitos "itens de plano de treino"
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}