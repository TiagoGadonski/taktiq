using GymHero.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Notifications.Commands;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, int>
{
    private readonly IApplicationDbContext _context;

    public MarkAllNotificationsAsReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var notifications = await _context.Notifications
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return notifications.Count;
    }
}
