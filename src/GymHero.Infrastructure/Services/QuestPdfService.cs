using GymHero.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GymHero.Infrastructure.Services;

public class QuestPdfService : IPdfGenerationService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<QuestPdfService> _logger;

    public QuestPdfService(
        IApplicationDbContext context,
        ILogger<QuestPdfService> logger)
    {
        _context = context;
        _logger = logger;

        // Set QuestPDF license (Community license is free for open-source projects)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateStudentProgressReportAsync(
        Guid studentId,
        Guid trainerId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var student = await _context.Users
            .Where(u => u.Id == studentId && u.PersonalTrainerId == trainerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (student == null)
        {
            throw new InvalidOperationException("Student not found or access denied");
        }

        var trainer = await _context.Users.FindAsync(new object[] { trainerId }, cancellationToken);

        var workoutsCount = await _context.WorkoutSessions
            .CountAsync(s => s.OwnerId == studentId && s.CompletedAt >= startDate && s.CompletedAt <= endDate, cancellationToken);

        var assessments = await _context.StudentAssessments
            .Where(a => a.StudentId == studentId && a.AssessmentDate >= startDate && a.AssessmentDate <= endDate)
            .OrderByDescending(a => a.AssessmentDate)
            .Take(5)
            .ToListAsync(cancellationToken);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("RELATÓRIO DE PROGRESSO").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(5).AlignCenter().Text($"{student.Name}").FontSize(14).SemiBold();
                    column.Item().AlignCenter().Text($"Período: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Item().PaddingBottom(10).Background(Colors.Grey.Lighten3).Padding(10)
                        .Text($"Total de Treinos: {workoutsCount}").FontSize(14).SemiBold();

                    if (assessments.Any())
                    {
                        column.Item().PaddingTop(15).Text("AVALIAÇÕES FÍSICAS").FontSize(12).Bold().FontColor(Colors.Blue.Darken1);
                        foreach (var assessment in assessments)
                        {
                            column.Item().PaddingTop(10).BorderLeft(3).BorderColor(Colors.Blue.Darken1).PaddingLeft(10).Column(assessCol =>
                            {
                                assessCol.Item().Text($"Data: {assessment.AssessmentDate:dd/MM/yyyy}").SemiBold();
                                if (assessment.BodyFatPercentage.HasValue)
                                    assessCol.Item().Text($"Gordura Corporal: {assessment.BodyFatPercentage:F1}%");
                                if (assessment.MuscleMass.HasValue)
                                    assessCol.Item().Text($"Massa Muscular: {assessment.MuscleMass:F1} kg");
                            });
                        }
                    }
                });

                page.Footer().AlignCenter().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    column.Item().PaddingTop(5).Text($"Gerado por: {trainer?.Name ?? "TaktIQ"} | {DateTime.UtcNow:dd/MM/yyyy HH:mm}").FontSize(9);
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateWorkoutPlanPdfAsync(
        Guid workoutPlanId,
        Guid trainerId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _context.WorkoutPlans
            .Include(p => p.Workouts).ThenInclude(w => w.Exercises).ThenInclude(we => we.Exercise)
            .Where(p => p.Id == workoutPlanId && p.OwnerId == trainerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (plan == null) throw new InvalidOperationException("Workout plan not found or access denied");

        var trainer = await _context.Users.FindAsync(new object[] { trainerId }, cancellationToken);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("PLANO DE TREINO").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(5).AlignCenter().Text(plan.Name).FontSize(14).SemiBold();
                    if (!string.IsNullOrEmpty(plan.Description))
                        column.Item().PaddingTop(5).AlignCenter().Text(plan.Description).FontSize(10).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    foreach (var workout in plan.Workouts.OrderBy(w => w.DayOfWeek))
                    {
                        column.Item().PageBreak();
                        column.Item().PaddingBottom(15).Column(workoutCol =>
                        {
                            workoutCol.Item().Background(Colors.Blue.Darken2).Padding(10).Text($"{workout.DayOfWeek} - {workout.Name}").FontSize(14).Bold().FontColor(Colors.White);

                            foreach (var exercise in workout.Exercises.OrderBy(e => e.Order))
                            {
                                workoutCol.Item().PaddingTop(10).Column(exCol =>
                                {
                                    exCol.Item().Text(exercise.Exercise?.Name ?? "N/A").FontSize(12).SemiBold();
                                    exCol.Item().Text($"{exercise.TargetSets} séries x {exercise.TargetReps} reps").FontSize(10);
                                    if (exercise.RestSeconds.HasValue)
                                        exCol.Item().Text($"Descanso: {exercise.RestSeconds}s").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    if (!string.IsNullOrEmpty(exercise.Notes))
                                        exCol.Item().Text($"Notas: {exercise.Notes}").FontSize(9).FontColor(Colors.Grey.Darken1);
                                });
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    column.Item().PaddingTop(5).Text($"Gerado por: {trainer?.Name ?? "TaktIQ"} | {DateTime.UtcNow:dd/MM/yyyy HH:mm}").FontSize(9);
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateAssessmentReportPdfAsync(
        Guid assessmentId,
        Guid trainerId,
        CancellationToken cancellationToken = default)
    {
        var assessment = await _context.StudentAssessments
            .Include(a => a.Student)
            .Where(a => a.Id == assessmentId && a.Student!.PersonalTrainerId == trainerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (assessment == null) throw new InvalidOperationException("Assessment not found or access denied");

        var trainer = await _context.Users.FindAsync(new object[] { trainerId }, cancellationToken);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("RELATÓRIO DE AVALIAÇÃO FÍSICA").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(5).AlignCenter().Text(assessment.Student?.Name ?? "N/A").FontSize(14).SemiBold();
                    column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Item().Text($"Data: {assessment.AssessmentDate:dd/MM/yyyy}").FontSize(12).SemiBold();
                    column.Item().PaddingTop(10).Column(col =>
                    {
                        if (assessment.BodyFatPercentage.HasValue)
                            col.Item().Text($"Gordura Corporal: {assessment.BodyFatPercentage:F1}%");
                        if (assessment.MuscleMass.HasValue)
                            col.Item().Text($"Massa Muscular: {assessment.MuscleMass:F1} kg");
                        if (assessment.FlexibilityScore.HasValue)
                            col.Item().Text($"Flexibilidade: {assessment.FlexibilityScore:F1}/10");
                    });
                });

                page.Footer().AlignCenter().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    column.Item().PaddingTop(5).Text($"Gerado por: {trainer?.Name ?? "TaktIQ"} | {DateTime.UtcNow:dd/MM/yyyy HH:mm}").FontSize(9);
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateMonthlySummaryReportAsync(
        Guid trainerId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var trainer = await _context.Users.FindAsync(new object[] { trainerId }, cancellationToken);
        var studentsCount = await _context.Users.CountAsync(u => u.PersonalTrainerId == trainerId, cancellationToken);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("RELATÓRIO MENSAL").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(5).AlignCenter().Text($"{startDate:MMMM yyyy}").FontSize(14).SemiBold();
                    column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    column.Item().Background(Colors.Blue.Lighten3).Padding(10).Text($"Total de Alunos: {studentsCount}").FontSize(14).Bold();
                });

                page.Footer().AlignCenter().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    column.Item().PaddingTop(5).Text($"Gerado por: {trainer?.Name ?? "TaktIQ"} | {DateTime.UtcNow:dd/MM/yyyy HH:mm}").FontSize(9);
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateAssessmentComparisonPdfAsync(
        Guid assessment1Id,
        Guid assessment2Id,
        Guid trainerId,
        CancellationToken cancellationToken = default)
    {
        var assessments = await _context.StudentAssessments
            .Include(a => a.Student)
            .Where(a => (a.Id == assessment1Id || a.Id == assessment2Id) && a.Student!.PersonalTrainerId == trainerId)
            .OrderBy(a => a.AssessmentDate)
            .ToListAsync(cancellationToken);

        if (assessments.Count != 2) throw new InvalidOperationException("Both assessments must exist and belong to the same trainer");

        var firstAssessment = assessments[0];
        var secondAssessment = assessments[1];
        var trainer = await _context.Users.FindAsync(new object[] { trainerId }, cancellationToken);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text("COMPARAÇÃO DE AVALIAÇÕES").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                    column.Item().PaddingTop(5).AlignCenter().Text(firstAssessment.Student?.Name ?? "N/A").FontSize(14).SemiBold();
                    column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(column =>
                {
                    if (firstAssessment.BodyFatPercentage.HasValue && secondAssessment.BodyFatPercentage.HasValue)
                    {
                        var diff = secondAssessment.BodyFatPercentage.Value - firstAssessment.BodyFatPercentage.Value;
                        column.Item().Text($"Gordura Corporal: {firstAssessment.BodyFatPercentage:F1}% → {secondAssessment.BodyFatPercentage:F1}% ({(diff >= 0 ? "+" : "")}{diff:F1}%)");
                    }
                    if (firstAssessment.MuscleMass.HasValue && secondAssessment.MuscleMass.HasValue)
                    {
                        var diff = secondAssessment.MuscleMass.Value - firstAssessment.MuscleMass.Value;
                        column.Item().Text($"Massa Muscular: {firstAssessment.MuscleMass:F1}kg → {secondAssessment.MuscleMass:F1}kg ({(diff >= 0 ? "+" : "")}{diff:F1}kg)");
                    }
                });

                page.Footer().AlignCenter().Column(column =>
                {
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    column.Item().PaddingTop(5).Text($"Gerado por: {trainer?.Name ?? "TaktIQ"} | {DateTime.UtcNow:dd/MM/yyyy HH:mm}").FontSize(9);
                });
            });
        });

        return document.GeneratePdf();
    }
}
