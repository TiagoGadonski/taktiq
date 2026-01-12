using GymHero.Shared.Enums;
using MediatR;

namespace GymHero.Application.Features.Exercises.Commands;

public record UpdateExerciseCommand(
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
    string? VideoUrl,
    string? ImageUrl,
    string? ThumbnailUrl,
    WorkoutLocation WorkoutLocation,
    string? SpaceRequired,
    List<string>? Progressions,
    List<string>? Regressions,
    string? NoEquipmentAlternative,
    bool IsPublic
) : IRequest<Unit>;
