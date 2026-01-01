using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Feedback.Queries;

public record GetSessionFeedbackQuery(
    Guid SessionId
) : IRequest<FeedbackResponse?>;

public class GetSessionFeedbackQueryHandler : IRequestHandler<GetSessionFeedbackQuery, FeedbackResponse?>
{
    private readonly IApplicationDbContext _context;

    public GetSessionFeedbackQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FeedbackResponse?> Handle(GetSessionFeedbackQuery request, CancellationToken cancellationToken)
    {
        var feedback = await _context.WorkoutSessionFeedbacks
            .FirstOrDefaultAsync(f => f.SessionId == request.SessionId, cancellationToken);

        if (feedback == null)
            return null;

        // Converter CSV de volta para listas
        var painAreas = !string.IsNullOrEmpty(feedback.PainAreas)
            ? feedback.PainAreas.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            : null;

        var favoriteExercises = !string.IsNullOrEmpty(feedback.FavoriteExercises)
            ? feedback.FavoriteExercises.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            : null;

        var dislikedExercises = !string.IsNullOrEmpty(feedback.DislikedExercises)
            ? feedback.DislikedExercises.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            : null;

        return new FeedbackResponse(
            feedback.Id,
            feedback.SessionId,
            feedback.UserId,
            feedback.DifficultyRating,
            feedback.EnergyLevel,
            feedback.OverallSatisfaction,
            painAreas,
            favoriteExercises,
            dislikedExercises,
            feedback.Comments,
            feedback.SubmittedAt
        );
    }
}
