using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Feedback.Commands;

public record SubmitSessionFeedbackCommand(
    Guid SessionId,
    Guid UserId,
    SubmitFeedbackRequest Request
) : IRequest<Guid>;

public class SubmitSessionFeedbackCommandHandler : IRequestHandler<SubmitSessionFeedbackCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public SubmitSessionFeedbackCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SubmitSessionFeedbackCommand request, CancellationToken cancellationToken)
    {
        // Verificar se a sessão existe e pertence ao usuário
        var session = await _context.WorkoutSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
            throw new KeyNotFoundException("Sessão de treino não encontrada");

        // Verificar se já existe feedback para esta sessão
        var existingFeedback = await _context.WorkoutSessionFeedbacks
            .FirstOrDefaultAsync(f => f.SessionId == request.SessionId, cancellationToken);

        if (existingFeedback != null)
            throw new InvalidOperationException("Feedback já foi enviado para esta sessão");

        var req = request.Request;

        // Converter listas para CSV
        var painAreasCSV = req.PainAreas != null && req.PainAreas.Any()
            ? string.Join(",", req.PainAreas)
            : null;

        var favoriteExercisesCSV = req.FavoriteExercises != null && req.FavoriteExercises.Any()
            ? string.Join(",", req.FavoriteExercises)
            : null;

        var dislikedExercisesCSV = req.DislikedExercises != null && req.DislikedExercises.Any()
            ? string.Join(",", req.DislikedExercises)
            : null;

        // Criar feedback
        var feedback = new WorkoutSessionFeedback
        {
            Id = Guid.NewGuid(),
            SessionId = request.SessionId,
            UserId = request.UserId,
            DifficultyRating = req.DifficultyRating,
            EnergyLevel = req.EnergyLevel,
            OverallSatisfaction = req.OverallSatisfaction,
            PainAreas = painAreasCSV,
            FavoriteExercises = favoriteExercisesCSV,
            DislikedExercises = dislikedExercisesCSV,
            Comments = req.Comments,
            SubmittedAt = DateTime.UtcNow
        };

        _context.WorkoutSessionFeedbacks.Add(feedback);
        await _context.SaveChangesAsync(cancellationToken);

        return feedback.Id;
    }
}
