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
            var locationFilter = (GymHero.Shared.Enums.WorkoutLocation)request.WorkoutLocation.Value;
            // Include exercises that match the requested location OR are marked as "Both"
            query = query.Where(e => e.WorkoutLocation == locationFilter || e.WorkoutLocation == GymHero.Shared.Enums.WorkoutLocation.Both);
        }

        // Filter by equipment if specified
        if (request.Equipment.HasValue)
        {
            var equipmentFilter = (GymHero.Shared.Enums.Equipment)request.Equipment.Value;
            query = query.Where(e => e.Equipment == equipmentFilter);
        }

        // Filter by muscle group if specified
        if (request.MuscleGroup.HasValue)
        {
            var muscleGroupFilter = (GymHero.Shared.Enums.MuscleGroup)request.MuscleGroup.Value;
            query = query.Where(e => e.MuscleGroup == muscleGroupFilter);
        }

        // Filter by difficulty if specified
        if (request.Difficulty.HasValue)
        {
            var difficultyFilter = (GymHero.Shared.Enums.DifficultyLevel)request.Difficulty.Value;
            query = query.Where(e => e.Difficulty == difficultyFilter);
        }

        // Filter by category if specified
        if (request.Category.HasValue)
        {
            var categoryFilter = (GymHero.Shared.Enums.ExerciseCategory)request.Category.Value;
            query = query.Where(e => e.Category == categoryFilter);
        }

        // Filter by search term if specified
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(searchTerm) ||
                                    (e.Description != null && e.Description.ToLower().Contains(searchTerm)));
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
                SecondaryMuscles = e.SecondaryMuscles,
                Difficulty = e.Difficulty,
                Instructions = e.Instructions,
                Tips = e.Tips,
                CommonMistakes = e.CommonMistakes,
                Notes = e.Notes,
                VideoUrl = e.VideoUrl,
                ImageUrl = e.ImageUrl,
                ThumbnailUrl = e.ThumbnailUrl,
                WorkoutLocation = e.WorkoutLocation,
                SpaceRequired = e.SpaceRequired,
                Progressions = e.Progressions,
                Regressions = e.Regressions,
                NoEquipmentAlternative = e.NoEquipmentAlternative,
                IsPublic = e.IsPublic,
                CreatedByUserId = e.CreatedByUserId,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}