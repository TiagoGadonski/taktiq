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
        var query = _context.Exercises.AsNoTracking();

        // Filter by workout location if specified
        if (request.WorkoutLocation.HasValue)
        {
            var locationFilter = (Domain.Enums.WorkoutLocation)request.WorkoutLocation.Value;
            // Include exercises that match the requested location OR are marked as "Both"
            query = query.Where(e => e.WorkoutLocation == locationFilter || e.WorkoutLocation == Domain.Enums.WorkoutLocation.Both);
        }

        return await query
            .Select(e => new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                MuscleGroup = e.MuscleGroup,
                Category = e.Category,
                Equipment = e.Equipment,
                Notes = e.Notes,
                VideoUrl = e.VideoUrl,
                ImageUrl = e.ImageUrl,
                WorkoutLocation = (int)e.WorkoutLocation
            })
            .ToListAsync(cancellationToken);
    }
}