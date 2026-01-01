using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Feedback.Queries;

public record GetStudentStatsQuery(
    Guid TrainerId,
    Guid StudentId,
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<StudentStatsResponse>;

public class GetStudentStatsQueryHandler : IRequestHandler<GetStudentStatsQuery, StudentStatsResponse>
{
    private readonly IApplicationDbContext _context;

    public GetStudentStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentStatsResponse> Handle(GetStudentStatsQuery request, CancellationToken cancellationToken)
    {
        // Verificar se o aluno pertence ao PT
        var student = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.StudentId, cancellationToken);

        if (student == null)
            throw new KeyNotFoundException("Aluno não encontrado");

        if (student.PersonalTrainerId != request.TrainerId)
            throw new UnauthorizedAccessException("Você não tem permissão para visualizar as estatísticas deste aluno");

        // Definir período
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Buscar todas as sessões do aluno no período (incluindo workouts e plans)
        var sessions = await _context.WorkoutSessions
            .Include(s => s.Workout)
                .ThenInclude(w => w!.Plan)
            .Include(s => s.Feedback)
            .Where(s => s.Workout!.Plan!.OwnerId == request.StudentId)
            .Where(s => s.StartedAt >= startDate && s.StartedAt <= endDate)
            .OrderByDescending(s => s.CompletedAt ?? s.StartedAt)
            .ToListAsync(cancellationToken);

        var totalSessions = sessions.Count;
        var completedSessions = sessions.Count(s => s.CompletedAt.HasValue);
        var completionRate = totalSessions > 0 ? (double)completedSessions / totalSessions * 100 : 0;

        // Buscar feedbacks
        var feedbacks = sessions
            .Where(s => s.Feedback != null)
            .Select(s => s.Feedback!)
            .ToList();

        var avgDifficulty = feedbacks.Any() ? (double?)feedbacks.Average(f => f.DifficultyRating) : null;
        var avgEnergy = feedbacks.Any() ? (double?)feedbacks.Average(f => f.EnergyLevel) : null;
        var avgSatisfaction = feedbacks.Any() ? (double?)feedbacks.Average(f => f.OverallSatisfaction) : null;

        // Áreas de dor frequentes
        var painAreasFlat = feedbacks
            .Where(f => !string.IsNullOrEmpty(f.PainAreas))
            .SelectMany(f => f.PainAreas!.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()))
            .ToList();

        var frequentPainAreas = painAreasFlat
            .GroupBy(p => p)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new PainAreaFrequency(
                g.Key,
                g.Count(),
                Math.Round((double)g.Count() / feedbacks.Count * 100, 1)
            ))
            .ToList();

        // Frequência por dia da semana
        var completedSessionsByDay = sessions
            .Where(s => s.CompletedAt.HasValue)
            .GroupBy(s => s.CompletedAt!.Value.DayOfWeek)
            .Select(g => new WorkoutFrequencyByDay(
                g.Key.ToString(),
                g.Count(),
                Math.Round((double)g.Count() / completedSessions * 100, 1)
            ))
            .OrderByDescending(f => f.Count)
            .ToList();

        // Feedback recente (últimos 10)
        var recentFeedback = sessions
            .Where(s => s.Feedback != null)
            .OrderByDescending(s => s.Feedback!.SubmittedAt)
            .Take(10)
            .Select(s => new RecentFeedbackSummary(
                s.Feedback!.SubmittedAt,
                s.Workout.Name,
                s.Feedback!.DifficultyRating,
                s.Feedback!.OverallSatisfaction,
                s.Feedback!.Comments
            ))
            .ToList();

        return new StudentStatsResponse(
            request.StudentId,
            student.Name,
            startDate,
            endDate,
            totalSessions,
            completedSessions,
            Math.Round(completionRate, 1),
            avgDifficulty.HasValue ? Math.Round(avgDifficulty.Value, 1) : null,
            avgEnergy.HasValue ? Math.Round(avgEnergy.Value, 1) : null,
            avgSatisfaction.HasValue ? Math.Round(avgSatisfaction.Value, 1) : null,
            frequentPainAreas,
            completedSessionsByDay,
            recentFeedback
        );
    }
}
