using GymHero.Shared.DTOs;
using GymHero.Domain.Enums;
using MediatR;
namespace GymHero.Application.Features.Exercises.Commands;

public record CreateExerciseCommand(string Name, string? Description, string MuscleGroup, string? Category, string? Equipment, string? Notes, string? VideoUrl, string? ImageUrl, WorkoutLocation WorkoutLocation = WorkoutLocation.Both) : IRequest<ExerciseDto>;