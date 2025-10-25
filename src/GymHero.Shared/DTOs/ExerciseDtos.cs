namespace GymHero.Shared.DTOs;

// DTO para exibir exercícios em listas na nossa UI
public record ExerciseResponse(
    Guid Id, 
    string Name, 
    string MuscleGroup, 
    string? Category,
    string? Equipment, 
    string? Notes,
    string? ImageUrl,
    string? VideoUrl
);

// DTO para a requisição de CRIAR um exercício
public class CreateExerciseRequest
{
    public string Name { get; set; } = "";
    public string MuscleGroup { get; set; } = "";
    public string? Category { get; set; }
    public string? Equipment { get; set; }
    public string? Notes { get; set; }
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
}

// DTO para a requisição de ATUALIZAR um exercício
public class UpdateExerciseRequest
{
    public string Name { get; set; } = "";
    public string MuscleGroup { get; set; } = "";
    public string? Category { get; set; }
    public string? Equipment { get; set; }
    public string? Notes { get; set; }
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
}

// DTO para o formulário no frontend
public class ExerciseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string MuscleGroup { get; set; } = "";
    public string? Category { get; set; }
    public string? Equipment { get; set; }
    public string? Notes { get; set; }
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
}