using System.ComponentModel.DataAnnotations;

namespace GymHero.Shared.DTOs;

// Dados que o usuário envia para criar um plano
public class CreateWorkoutPlanRequest
{
    [Required(ErrorMessage = "O nome do plano é obrigatório")]
    public string Name { get; set; } = "";
    public string? Goal { get; set; }
}

// Dados que retornamos após a criação
public record WorkoutPlanResponse(Guid Id, Guid OwnerId, string Name, string? Goal, bool IsActive, DateTime CreatedAt);

public record UpdateWorkoutPlanRequest(string Name, string? Goal);

public record AddExerciseToPlanRequest(
    Guid ExerciseId,
    int Order,
    int TargetSets,
    int TargetReps,
    double TargetLoad
);

public record AddWorkoutToPlanRequest(
    string Name,
    int? DayOfWeek,
    int Order
);

public record AddExerciseToWorkoutRequest(
    Guid ExerciseId,
    int Order,
    int TargetSets,
    int TargetReps,
    double TargetLoad
);

public class WorkoutPlanDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Goal { get; set; }
    public bool IsActive { get; set; }
    public List<WorkoutDto> Workouts { get; set; } = new();
    public List<WorkoutExerciseDto> Exercises { get; set; } = new();
}

public class WorkoutDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int? DayOfWeek { get; set; }
    public int Order { get; set; }
    public List<WorkoutExerciseDto> Exercises { get; set; } = new();
}

public class WorkoutExerciseDto
{
    public Guid Id { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = "";
    public ExerciseDto? Exercise { get; set; }
    public int Order { get; set; }
    public int TargetSets { get; set; }
    public int TargetReps { get; set; }
    public double TargetLoad { get; set; }
}

public class ShareWorkoutPlanRequest
{
    [Required(ErrorMessage = "Selecione pelo menos um amigo para compartilhar")]
    public List<Guid> FriendIds { get; set; } = new();
}