using GymHero.Application.Common.Interfaces;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Application.Features.Notifications.Queries;

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, IEnumerable<NotificationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserNotificationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == request.UserId);

        if (request.UnreadOnly == true)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50) // Limit to last 50 notifications
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                Data = n.Data,
                ActionUrl = n.ActionUrl,
                IsRead = n.IsRead,
                ReadAt = n.ReadAt,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return notifications;
    }
}
