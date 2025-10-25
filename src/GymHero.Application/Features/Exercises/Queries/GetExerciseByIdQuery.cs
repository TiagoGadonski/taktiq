using GymHero.Shared.DTOs;
using MediatR;
namespace GymHero.Application.Features.Exercises.Queries;

public record GetExerciseByIdQuery(Guid Id) : IRequest<ExerciseDto?>;