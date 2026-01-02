using MediatR;
using Microsoft.EntityFrameworkCore;
using GymHero.Infrastructure.Data;

namespace GymHero.Application.Features.Feedback.Queries;

public record GetUnreadFeedbackCountQuery(Guid TrainerId) : IRequest<UnreadFeedbackCountResponse>;

public record UnreadFeedbackCountResponse(
    int UnreadCount,
    DateTime? LastFeedbackDate
);

public class GetUnreadFeedbackCountQueryHandler : IRequestHandler<GetUnreadFeedbackCountQuery, UnreadFeedbackCountResponse>
{
    private readonly ApplicationDbContext _context;

    public GetUnreadFeedbackCountQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UnreadFeedbackCountResponse> Handle(GetUnreadFeedbackCountQuery request, CancellationToken cancellationToken)
    {
        // Get all students for this PT
        var studentIds = await _context.Users
            .Where(u => u.PersonalTrainerId == request.TrainerId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (!studentIds.Any())
        {
            return new UnreadFeedbackCountResponse(0, null);
        }

        // Get feedback from last 7 days for PT's students
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var recentFeedback = await _context.WorkoutSessionFeedback
            .Where(f => f.SubmittedAt >= sevenDaysAgo)
            .Where(f => studentIds.Contains(f.UserId))
            .OrderByDescending(f => f.SubmittedAt)
            .ToListAsync(cancellationToken);

        var unreadCount = recentFeedback.Count;
        var lastFeedbackDate = recentFeedback.FirstOrDefault()?.SubmittedAt;

        return new UnreadFeedbackCountResponse(unreadCount, lastFeedbackDate);
    }
}
