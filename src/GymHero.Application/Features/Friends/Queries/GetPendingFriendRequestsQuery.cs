using GymHero.Shared.DTOs;
using MediatR;

namespace GymHero.Application.Features.Friends.Queries;

/// <summary>
/// Representa uma consulta para obter todos os pedidos de amizade pendentes
/// para um utilizador específico (o destinatário).
/// </summary>
public record GetPendingFriendRequestsQuery(Guid UserId) : IRequest<IEnumerable<FriendRequestResponse>>;