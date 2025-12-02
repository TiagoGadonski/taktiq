namespace GymHero.Shared.DTOs;

// DTO para exibir exercícios em listas na nossa UI
public record ExerciseResponse(
    Guid Id,
    string Name,
    string? Description,
    string MuscleGroup,
    string? Category,
    string? Equipment,
    string? Notes,
    string? ImageUrl,
    string? VideoUrl,
    int WorkoutLocation // 0 = Gym, 1 = Home, 2 = Both
);

// DTO para a requisição de CRIAR um exercício
public class CreateExerciseRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string MuscleGroup { get; set; } = "";
    public string? Category { get; set; }
    public string? Equipment { get; set; }
    public string? Notes { get; set; }
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int WorkoutLocation { get; set; } = 2; // Default to Both
}

// DTO para a requisição de ATUALIZAR um exercício
public class UpdateExerciseRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string MuscleGroup { get; set; } = "";
    public string? Category { get; set; }
    public string? Equipment { get; set; }
    public string? Notes { get; set; }
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int WorkoutLocation { get; set; } = 2; // Default to Both
}

// DTO para o formulário no frontend
public class ExerciseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string MuscleGroup { get; set; } = "";
    public string? Category { get; set; }
    public string? Equipment { get; set; }
    public string? Notes { get; set; }
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int WorkoutLocation { get; set; } = 2; // 0 = Gym, 1 = Home, 2 = Both
}