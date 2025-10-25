using GymHero.Application.Common.Exceptions;
using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Exercises.Commands;

public class DeleteExerciseCommandHandler : IRequestHandler<DeleteExerciseCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteExerciseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (exercise is null)
        {
            throw new NotFoundException("Exercise not found.");
        }

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync(cancellationToken);
    }
}