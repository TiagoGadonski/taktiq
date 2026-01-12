using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Exercises.Queries;

public record GetAllExercisesQuery(
    int? WorkoutLocation = null,
    int? Equipment = null,
    int? MuscleGroup = null,
    int? Difficulty = null,
    int? Category = null,
    string? Search = null) : IRequest<IEnumerable<ExerciseDto>>;