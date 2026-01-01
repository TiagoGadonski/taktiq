using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GymHero.Application.Features.Assessments.Commands;

public record CreateAssessmentCommand(
    Guid TrainerId,
    CreateAssessmentRequest Request
) : IRequest<Guid>;

public class CreateAssessmentCommandHandler : IRequestHandler<CreateAssessmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateAssessmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateAssessmentCommand request, CancellationToken cancellationToken)
    {
        var cmd = request.Request;

        // Verificar se o aluno pertence ao PT
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == cmd.StudentId, cancellationToken);

        if (student == null)
            throw new UnauthorizedAccessException("Aluno não encontrado");

        if (student.PersonalTrainerId != request.TrainerId)
            throw new UnauthorizedAccessException("Você não tem permissão para avaliar este aluno");

        // Desativar avaliações anteriores do mesmo tipo
        var previousAssessments = await _context.StudentAssessments
            .Where(a => a.StudentId == cmd.StudentId && a.AssessmentType == cmd.AssessmentType && a.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var prev in previousAssessments)
        {
            prev.IsActive = false;
        }

        // Serializar campos customizados se existirem
        string? customFieldsJson = null;
        if (cmd.CustomFields != null && cmd.CustomFields.Any())
        {
            customFieldsJson = JsonSerializer.Serialize(cmd.CustomFields);
        }

        // Criar nova avaliação
        var assessment = new StudentAssessment
        {
            Id = Guid.NewGuid(),
            StudentId = cmd.StudentId,
            TrainerId = request.TrainerId,
            AssessmentType = cmd.AssessmentType,
            AssessmentDate = DateTime.UtcNow,
            IsActive = true,

            // Postural
            ForwardHead = cmd.ForwardHead,
            RoundedShoulders = cmd.RoundedShoulders,
            AnteriorPelvicTilt = cmd.AnteriorPelvicTilt,
            PosteriorPelvicTilt = cmd.PosteriorPelvicTilt,
            KneeValgus = cmd.KneeValgus,
            KneeVarus = cmd.KneeVarus,
            FlatFeet = cmd.FlatFeet,
            Scoliosis = cmd.Scoliosis,

            // Physical
            BodyFatPercentage = cmd.BodyFatPercentage,
            MuscleMass = cmd.MuscleMass,
            FlexibilityScore = cmd.FlexibilityScore,
            StrengthScore = cmd.StrengthScore,
            CardioScore = cmd.CardioScore,

            // Custom & Notes
            CustomFields = customFieldsJson,
            TrainerNotes = cmd.TrainerNotes,

            // Recommendations serão geradas pela AI posteriormente
            Recommendations = null
        };

        _context.StudentAssessments.Add(assessment);
        await _context.SaveChangesAsync(cancellationToken);

        return assessment.Id;
    }
}
