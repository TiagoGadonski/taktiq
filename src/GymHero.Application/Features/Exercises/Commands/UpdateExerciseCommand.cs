using MediatR;
namespace GymHero.Application.Features.Exercises.Commands;

public record UpdateExerciseCommand(Guid Id, string Name, string? Description, string MuscleGroup, string? Category, string? Equipment, string? Notes, string? VideoUrl, string? ImageUrl) : IRequest;