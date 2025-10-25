using MediatR;

namespace GymHero.Application.Features.WorkoutPlans.Commands;

public record AddExerciseToWorkoutCommand(
    Guid WorkoutId,
    Guid OwnerId,
    Guid ExerciseId,
    int Order,
    int TargetSets,
    int TargetReps,
    double TargetLoad
) : IRequest<Guid>;
