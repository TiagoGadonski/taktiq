using GymHero.Shared.DTOs;
using GymHero.Shared.Enums;
using MediatR;

namespace GymHero.Application.Features.Exercises.Commands;

public record CreateExerciseCommand(
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
    WorkoutLocation WorkoutLocation = WorkoutLocation.Both,
    string? SpaceRequired = null,
    List<string>? Progressions = null,
    List<string>? Regressions = null,
    string? NoEquipmentAlternative = null,
    bool IsPublic = true
) : IRequest<ExerciseDto>;
