using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
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
            SecondaryMuscles = request.SecondaryMuscles,
            Equipment = request.Equipment,
            Category = request.Category,
            Difficulty = request.Difficulty,
            Instructions = request.Instructions,
            Tips = request.Tips,
            CommonMistakes = request.CommonMistakes,
            Notes = request.Notes,
            VideoUrl = request.VideoUrl,
            ImageUrl = request.ImageUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            WorkoutLocation = request.WorkoutLocation,
            SpaceRequired = request.SpaceRequired,
            Progressions = request.Progressions,
            Regressions = request.Regressions,
            NoEquipmentAlternative = request.NoEquipmentAlternative,
            IsPublic = request.IsPublic,
            CreatedAt = DateTime.UtcNow
        };

        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync(cancellationToken);

        return new ExerciseDto
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            MuscleGroup = exercise.MuscleGroup,
            SecondaryMuscles = exercise.SecondaryMuscles,
            Equipment = exercise.Equipment,
            Category = exercise.Category,
            Difficulty = exercise.Difficulty,
            Instructions = exercise.Instructions,
            Tips = exercise.Tips,
            CommonMistakes = exercise.CommonMistakes,
            Notes = exercise.Notes,
            VideoUrl = exercise.VideoUrl,
            ImageUrl = exercise.ImageUrl,
            ThumbnailUrl = exercise.ThumbnailUrl,
            WorkoutLocation = exercise.WorkoutLocation,
            SpaceRequired = exercise.SpaceRequired,
            Progressions = exercise.Progressions,
            Regressions = exercise.Regressions,
            NoEquipmentAlternative = exercise.NoEquipmentAlternative,
            IsPublic = exercise.IsPublic,
            CreatedByUserId = exercise.CreatedByUserId,
            CreatedAt = exercise.CreatedAt,
            UpdatedAt = exercise.UpdatedAt
        };
    }
}
