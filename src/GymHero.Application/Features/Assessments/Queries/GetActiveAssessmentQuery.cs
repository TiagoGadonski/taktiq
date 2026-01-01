using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Assessments.Queries;

// Query para buscar a avaliação ativa mais recente de um usuário
// Usada pela AI para considerar desvios posturais ao gerar treinos
public record GetActiveAssessmentQuery(
    Guid StudentId
) : IRequest<StudentAssessment?>;

public class GetActiveAssessmentQueryHandler : IRequestHandler<GetActiveAssessmentQuery, StudentAssessment?>
{
    private readonly IApplicationDbContext _context;

    public GetActiveAssessmentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentAssessment?> Handle(GetActiveAssessmentQuery request, CancellationToken cancellationToken)
    {
        // Busca a avaliação ativa mais recente
        // Prioriza avaliações do tipo "Postural" pois são as que mais impactam na seleção de exercícios
        var assessment = await _context.StudentAssessments
            .Where(a => a.StudentId == request.StudentId && a.IsActive)
            .OrderByDescending(a => a.AssessmentType == "Postural" ? 1 : 0)  // Prioriza Postural
            .ThenByDescending(a => a.AssessmentDate)  // Mais recente
            .FirstOrDefaultAsync(cancellationToken);

        return assessment;
    }
}
