using MediatR;

namespace GymHero.Application.Features.Notifications.Commands;

public record MarkAllNotificationsAsReadCommand(
    Guid UserId
) : IRequest<int>;
