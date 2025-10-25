using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Exercises.Queries;

public class GetExerciseByIdQueryHandler : IRequestHandler<GetExerciseByIdQuery, ExerciseDto?>
{
    private readonly IApplicationDbContext _context;
    public GetExerciseByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ExerciseDto?> Handle(GetExerciseByIdQuery request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (exercise is null)
        {
            return null;
        }

        return new ExerciseDto
{
    Id = exercise.Id,
    Name = exercise.Name,
    MuscleGroup = exercise.MuscleGroup,
    Category = exercise.Category,
    Equipment = exercise.Equipment,
    Notes = exercise.Notes
};
    }
}