using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using GymHero.Domain.Entities;
using MediatR;

namespace GymHero.Application.Features.Exercises.Commands;

public class CreateExerciseCommandHandler : IRequestHandler<CreateExerciseCommand, ExerciseDto>
{
    private readonly IApplicationDbContext _context;

    public CreateExerciseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ExerciseDto> Handle(CreateExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = new Exercise
        {
            Name = request.Name,
            Description = request.Description,
            MuscleGroup = request.MuscleGroup,
            Category = request.Category,
            Equipment = request.Equipment,
            Notes = request.Notes,
            VideoUrl = request.VideoUrl,
            ImageUrl = request.ImageUrl,
            WorkoutLocation = request.WorkoutLocation
        };

        await _context.Exercises.AddAsync(exercise, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ExerciseDto
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            MuscleGroup = exercise.MuscleGroup,
            Category = exercise.Category,
            Equipment = exercise.Equipment,
            Notes = exercise.Notes,
            VideoUrl = exercise.VideoUrl,
            ImageUrl = exercise.ImageUrl,
            WorkoutLocation = (int)exercise.WorkoutLocation
        };
    }
}