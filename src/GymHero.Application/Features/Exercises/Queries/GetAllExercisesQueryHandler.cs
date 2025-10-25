using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Exercises.Queries;

public class GetAllExercisesQueryHandler : IRequestHandler<GetAllExercisesQuery, IEnumerable<ExerciseDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAllExercisesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<ExerciseDto>> Handle(GetAllExercisesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Exercises
            .AsNoTracking()
            .Select(e => new ExerciseDto
{
    Id = e.Id,
    Name = e.Name,
    MuscleGroup = e.MuscleGroup,
    Category = e.Category,
    Equipment = e.Equipment,
    Notes = e.Notes
})
            .ToListAsync(cancellationToken);
    }
}