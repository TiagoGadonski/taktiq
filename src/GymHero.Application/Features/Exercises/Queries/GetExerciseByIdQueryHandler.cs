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
            Description = exercise.Description,
            MuscleGroup = exercise.MuscleGroup,
            Category = exercise.Category,
            Equipment = exercise.Equipment,
            SecondaryMuscles = exercise.SecondaryMuscles ?? new List<GymHero.Shared.Enums.MuscleGroup>(),
            Difficulty = exercise.Difficulty,
            Instructions = exercise.Instructions ?? new List<string>(),
            Tips = exercise.Tips ?? new List<string>(),
            CommonMistakes = exercise.CommonMistakes ?? new List<string>(),
            Notes = exercise.Notes,
            VideoUrl = exercise.VideoUrl,
            ImageUrl = exercise.ImageUrl,
            ThumbnailUrl = exercise.ThumbnailUrl,
            WorkoutLocation = exercise.WorkoutLocation,
            SpaceRequired = exercise.SpaceRequired ?? string.Empty,
            Progressions = exercise.Progressions ?? new List<string>(),
            Regressions = exercise.Regressions ?? new List<string>(),
            NoEquipmentAlternative = exercise.NoEquipmentAlternative ?? string.Empty,
            IsPublic = exercise.IsPublic,
            CreatedByUserId = exercise.CreatedByUserId,
            CreatedAt = exercise.CreatedAt,
            UpdatedAt = exercise.UpdatedAt
        };
    }
}