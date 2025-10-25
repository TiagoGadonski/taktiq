using MediatR;
namespace GymHero.Application.Features.Exercises.Commands;

public record DeleteExerciseCommand(Guid Id) : IRequest;