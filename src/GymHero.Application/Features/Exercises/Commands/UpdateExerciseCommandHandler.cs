using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Exercises.Commands;

public class UpdateExerciseCommandHandler : IRequestHandler<UpdateExerciseCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateExerciseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (exercise is null)
        {
            throw new NotFoundException("Exercise not found.");
        }

        exercise.Name = request.Name;
        exercise.MuscleGroup = request.MuscleGroup;
        exercise.Category = request.Category;
        exercise.Equipment = request.Equipment;
        exercise.Notes = request.Notes;
        exercise.VideoUrl = request.VideoUrl;
        exercise.ImageUrl = request.ImageUrl;

        await _context.SaveChangesAsync(cancellationToken);
    }
}