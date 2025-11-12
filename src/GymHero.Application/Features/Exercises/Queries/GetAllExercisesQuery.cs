using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Exercises.Queries;
public record GetAllExercisesQuery(int? WorkoutLocation = null) : IRequest<IEnumerable<ExerciseDto>>;