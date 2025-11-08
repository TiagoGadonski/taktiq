using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Notifications.Queries;

public record GetUserNotificationsQuery(
    Guid UserId,
    bool? UnreadOnly = null
) : IRequest<IEnumerable<NotificationDto>>;
