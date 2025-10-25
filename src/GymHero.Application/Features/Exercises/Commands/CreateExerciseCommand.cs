using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Exercises.Commands;

public record CreateExerciseCommand(string Name, string MuscleGroup, string? Category, string? Equipment, string? Notes, string? VideoUrl, string? ImageUrl) : IRequest<ExerciseDto>;