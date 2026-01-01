using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Assessments.Queries;

public record GetStudentAssessmentsQuery(
    Guid TrainerId,
    Guid StudentId
) : IRequest<IEnumerable<AssessmentResponse>>;

public class GetStudentAssessmentsQueryHandler : IRequestHandler<GetStudentAssessmentsQuery, IEnumerable<AssessmentResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetStudentAssessmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AssessmentResponse>> Handle(GetStudentAssessmentsQuery request, CancellationToken cancellationToken)
    {
        // Verificar se o aluno pertence ao PT
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.StudentId, cancellationToken);

        if (student == null)
            throw new KeyNotFoundException("Aluno não encontrado");

        if (student.PersonalTrainerId != request.TrainerId)
            throw new UnauthorizedAccessException("Você não tem permissão para visualizar as avaliações deste aluno");

        var assessments = await _context.StudentAssessments
            .Where(a => a.StudentId == request.StudentId)
            .OrderByDescending(a => a.AssessmentDate)
            .Select(a => new AssessmentResponse(
                a.Id,
                a.StudentId,
                a.Student.Name,
                a.Student.Email,
                a.AssessmentType,
                a.AssessmentDate,
                a.IsActive,
                GenerateSummary(a)  // Método helper para gerar resumo
            ))
            .ToListAsync(cancellationToken);

        return assessments;
    }

    private static string GenerateSummary(Domain.Entities.StudentAssessment assessment)
    {
        var issues = new List<string>();

        // Verificar desvios posturais moderados/severos
        if (assessment.ForwardHead is "Moderate" or "Severe")
            issues.Add("Cabeça anteriorizada");

        if (assessment.RoundedShoulders is "Moderate" or "Severe")
            issues.Add("Ombros protusos");

        if (assessment.AnteriorPelvicTilt is "Moderate" or "Severe")
            issues.Add("Inclinação pélvica anterior");

        if (assessment.PosteriorPelvicTilt is "Moderate" or "Severe")
            issues.Add("Inclinação pélvica posterior");

        if (assessment.KneeValgus is "Moderate" or "Severe")
            issues.Add("Joelhos valgos");

        if (assessment.FlatFeet is "Moderate" or "Severe")
            issues.Add("Pés planos");

        if (assessment.Scoliosis is "Moderate" or "Severe")
            issues.Add("Escoliose");

        // Verificar scores baixos
        if (assessment.FlexibilityScore.HasValue && assessment.FlexibilityScore < 5)
            issues.Add("Flexibilidade baixa");

        if (assessment.StrengthScore.HasValue && assessment.StrengthScore < 5)
            issues.Add("Força baixa");

        if (assessment.CardioScore.HasValue && assessment.CardioScore < 5)
            issues.Add("Condicionamento cardiovascular baixo");

        if (!issues.Any())
            return "Sem desvios significativos identificados";

        return string.Join(", ", issues);
    }
}
