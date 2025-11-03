using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public record ReplaceExerciseInWorkoutCommand(
    Guid WorkoutExerciseId,
    Guid OwnerId,
    Guid NewExerciseId,
    int? TargetSets = null,
    int? TargetReps = null,
    double? TargetLoad = null
) : IRequest;
