using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GymHero.Application.Features.Assessments.Commands;

public record UpdateAssessmentCommand(
    Guid AssessmentId,
    Guid TrainerId,
    UpdateAssessmentRequest Request
) : IRequest<Unit>;

public class UpdateAssessmentCommandHandler : IRequestHandler<UpdateAssessmentCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public UpdateAssessmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateAssessmentCommand request, CancellationToken cancellationToken)
    {
        var assessment = await _context.StudentAssessments
            .FirstOrDefaultAsync(a => a.Id == request.AssessmentId, cancellationToken);

        if (assessment == null)
            throw new KeyNotFoundException("Avaliação não encontrada");

        if (assessment.TrainerId != request.TrainerId)
            throw new UnauthorizedAccessException("Você não tem permissão para editar esta avaliação");

        var req = request.Request;

        // Atualizar campos
        assessment.AssessmentType = req.AssessmentType;

        // Postural
        assessment.ForwardHead = req.ForwardHead;
        assessment.RoundedShoulders = req.RoundedShoulders;
        assessment.AnteriorPelvicTilt = req.AnteriorPelvicTilt;
        assessment.PosteriorPelvicTilt = req.PosteriorPelvicTilt;
        assessment.KneeValgus = req.KneeValgus;
        assessment.KneeVarus = req.KneeVarus;
        assessment.FlatFeet = req.FlatFeet;
        assessment.Scoliosis = req.Scoliosis;

        // Physical
        assessment.BodyFatPercentage = req.BodyFatPercentage;
        assessment.MuscleMass = req.MuscleMass;
        assessment.FlexibilityScore = req.FlexibilityScore;
        assessment.StrengthScore = req.StrengthScore;
        assessment.CardioScore = req.CardioScore;

        // Custom fields
        if (req.CustomFields != null && req.CustomFields.Any())
        {
            assessment.CustomFields = JsonSerializer.Serialize(req.CustomFields);
        }
        else
        {
            assessment.CustomFields = null;
        }

        assessment.TrainerNotes = req.TrainerNotes;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
