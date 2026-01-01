using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Assessments.Commands;

public record DeleteAssessmentCommand(
    Guid AssessmentId,
    Guid TrainerId
) : IRequest<Unit>;

public class DeleteAssessmentCommandHandler : IRequestHandler<DeleteAssessmentCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteAssessmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _context.StudentAssessments
            .FirstOrDefaultAsync(a => a.Id == request.AssessmentId, cancellationToken);

        if (assessment == null)
            throw new KeyNotFoundException("Avaliação não encontrada");

        if (assessment.TrainerId != request.TrainerId)
            throw new UnauthorizedAccessException("Você não tem permissão para deletar esta avaliação");

        _context.StudentAssessments.Remove(assessment);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
