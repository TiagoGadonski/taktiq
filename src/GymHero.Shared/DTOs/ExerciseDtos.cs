using GymHero.Shared.Enums;

namespace GymHero.Shared.DTOs;

// DTO para exibir exercícios em listas na nossa UI
public record ExerciseResponse(
    Guid Id,
    string Name,
    string? Description,
    MuscleGroup MuscleGroup,
    List<MuscleGroup>? SecondaryMuscles,
    Equipment Equipment,
    ExerciseCategory Category,
    DifficultyLevel Difficulty,
    List<string>? Instructions,
    List<string>? Tips,
    List<string>? CommonMistakes,
    string? Notes,
    string? ImageUrl,
    string? VideoUrl,
    string? ThumbnailUrl,
    WorkoutLocation WorkoutLocation,
    string? SpaceRequired,
    List<string>? Progressions,
    List<string>? Regressions,
    string? NoEquipmentAlternative,
    bool IsPublic,
    Guid? CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// DTO para a requisição de CRIAR um exercício
public class CreateExerciseRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public MuscleGroup MuscleGroup { get; set; }
    public List<MuscleGroup>? SecondaryMuscles { get; set; }
    public Equipment Equipment { get; set; }
    public ExerciseCategory Category { get; set; }
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Beginner;
    public List<string>? Instructions { get; set; }
    public List<string>? Tips { get; set; }
    public List<string>? CommonMistakes { get; set; }
    public string? Notes { get; set; }
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public WorkoutLocation WorkoutLocation { get; set; } = WorkoutLocation.Both;
    public string? SpaceRequired { get; set; }
    public List<string>? Progressions { get; set; }
    public List<string>? Regressions { get; set; }
    public string? NoEquipmentAlternative { get; set; }
    public bool IsPublic { get; set; } = true;
}

// DTO para a requisição de ATUALIZAR um exercício
public class UpdateExerciseRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public MuscleGroup MuscleGroup { get; set; }
    public List<MuscleGroup>? SecondaryMuscles { get; set; }
    public Equipment Equipment { get; set; }
    public ExerciseCategory Category { get; set; }
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Beginner;
    public List<string>? Instructions { get; set; }
    public List<string>? Tips { get; set; }
    public List<string>? CommonMistakes { get; set; }
    public string? Notes { get; set; }
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public WorkoutLocation WorkoutLocation { get; set; } = WorkoutLocation.Both;
    public string? SpaceRequired { get; set; }
    public List<string>? Progressions { get; set; }
    public List<string>? Regressions { get; set; }
    public string? NoEquipmentAlternative { get; set; }
    public bool IsPublic { get; set; } = true;
}

// DTO para o formulário no frontend
public class ExerciseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public MuscleGroup MuscleGroup { get; set; }
    public List<MuscleGroup>? SecondaryMuscles { get; set; }
    public Equipment Equipment { get; set; }
    public ExerciseCategory Category { get; set; }
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Beginner;
    public List<string>? Instructions { get; set; }
    public List<string>? Tips { get; set; }
    public List<string>? CommonMistakes { get; set; }
    public string? Notes { get; set; }
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public WorkoutLocation WorkoutLocation { get; set; } = WorkoutLocation.Both;
    public string? SpaceRequired { get; set; }
    public List<string>? Progressions { get; set; }
    public List<string>? Regressions { get; set; }
    public string? NoEquipmentAlternative { get; set; }
    public bool IsPublic { get; set; } = true;
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
