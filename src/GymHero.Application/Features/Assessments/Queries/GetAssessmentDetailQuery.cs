using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GymHero.Application.Features.Assessments.Queries;

public record GetAssessmentDetailQuery(
    Guid AssessmentId,
    Guid TrainerId
) : IRequest<AssessmentDetailResponse?>;

public class GetAssessmentDetailQueryHandler : IRequestHandler<GetAssessmentDetailQuery, AssessmentDetailResponse?>
{
    private readonly IApplicationDbContext _context;

    public GetAssessmentDetailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AssessmentDetailResponse?> Handle(GetAssessmentDetailQuery request, CancellationToken cancellationToken)
    {
        var assessment = await _context.StudentAssessments
            .Include(a => a.Student)
            .Include(a => a.Trainer)
            .FirstOrDefaultAsync(a => a.Id == request.AssessmentId, cancellationToken);

        if (assessment == null)
            return null;

        if (assessment.TrainerId != request.TrainerId)
            throw new UnauthorizedAccessException("Você não tem permissão para visualizar esta avaliação");

        // Deserializar campos customizados
        List<CustomFieldDto>? customFields = null;
        if (!string.IsNullOrEmpty(assessment.CustomFields))
        {
            try
            {
                customFields = JsonSerializer.Deserialize<List<CustomFieldDto>>(assessment.CustomFields);
            }
            catch
            {
                // Se falhar deserialização, ignora
            }
        }

        return new AssessmentDetailResponse(
            assessment.Id,
            assessment.StudentId,
            assessment.Student.Name,
            assessment.Student.Email,
            assessment.TrainerId,
            assessment.Trainer.Name,
            assessment.AssessmentType,
            assessment.AssessmentDate,
            assessment.IsActive,

            // Postural
            assessment.ForwardHead,
            assessment.RoundedShoulders,
            assessment.AnteriorPelvicTilt,
            assessment.PosteriorPelvicTilt,
            assessment.KneeValgus,
            assessment.KneeVarus,
            assessment.FlatFeet,
            assessment.Scoliosis,

            // Physical
            assessment.BodyFatPercentage,
            assessment.MuscleMass,
            assessment.FlexibilityScore,
            assessment.StrengthScore,
            assessment.CardioScore,

            // Custom & Notes
            customFields,
            assessment.TrainerNotes,
            assessment.Recommendations
        );
    }
}
