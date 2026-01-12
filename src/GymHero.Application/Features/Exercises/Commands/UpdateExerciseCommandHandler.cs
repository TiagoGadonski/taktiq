using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Exercises.Commands;

public class UpdateExerciseCommandHandler : IRequestHandler<UpdateExerciseCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateExerciseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Unit> Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (exercise is null)
        {
            throw new NotFoundException("Exercise not found.");
        }

        exercise.Name = request.Name;
        exercise.Description = request.Description;
        exercise.MuscleGroup = request.MuscleGroup;
        exercise.SecondaryMuscles = request.SecondaryMuscles;
        exercise.Equipment = request.Equipment;
        exercise.Category = request.Category;
        exercise.Difficulty = request.Difficulty;
        exercise.Instructions = request.Instructions;
        exercise.Tips = request.Tips;
        exercise.CommonMistakes = request.CommonMistakes;
        exercise.Notes = request.Notes;
        exercise.VideoUrl = request.VideoUrl;
        exercise.ImageUrl = request.ImageUrl;
        exercise.ThumbnailUrl = request.ThumbnailUrl;
        exercise.WorkoutLocation = request.WorkoutLocation;
        exercise.SpaceRequired = request.SpaceRequired;
        exercise.Progressions = request.Progressions;
        exercise.Regressions = request.Regressions;
        exercise.NoEquipmentAlternative = request.NoEquipmentAlternative;
        exercise.IsPublic = request.IsPublic;
        exercise.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
