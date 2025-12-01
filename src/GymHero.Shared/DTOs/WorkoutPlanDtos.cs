using System.ComponentModel.DataAnnotations;
using GymHero.Shared.Enums;

namespace GymHero.Shared.DTOs;

// Dados que o usuário envia para criar um plano
public class CreateWorkoutPlanRequest
{
    [Required(ErrorMessage = "O nome do plano é obrigatório")]
    public string Name { get; set; } = "";
    public string? Goal { get; set; }
    public int? Duration { get; set; } // Duração em semanas

    // PT-specific fields
    public Guid? AssignedToUserId { get; set; } // Para atribuir a um aluno
    public DateTime? ExpirationDate { get; set; }

    // Marketplace fields
    public bool ForSale { get; set; } = false;
    public decimal? Price { get; set; }
    public bool IsPublic { get; set; } = false;
}

// Dados que retornamos após a criação
public record WorkoutPlanResponse(
    Guid Id,
    Guid OwnerId,
    string Name,
    string? Goal,
    bool IsActive,
    DateTime CreatedAt,
    int? Duration = null,
    DateTime? StartDate = null,
    DateTime? ExpirationDate = null
);

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

public record ReplaceExerciseRequest(
    Guid NewExerciseId,
    int? TargetSets = null,
    int? TargetReps = null,
    double? TargetLoad = null
);

public class WorkoutPlanDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Goal { get; set; }
    public bool IsActive { get; set; }
    public int? Duration { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
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

public record UpdateVisibilityRequest(
    VisibilityLevel VisibilityLevel,
    bool AllowCopying
);

public record RenewWorkoutPlanRequest(
    int AdditionalWeeks
);

public record DuplicateWorkoutPlanRequest(
    int Duration
);

public record UpdateMarketplaceSettingsRequest(
    bool ForSale,
    decimal? Price
);