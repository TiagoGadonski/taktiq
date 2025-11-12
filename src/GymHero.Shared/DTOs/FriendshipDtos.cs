namespace GymHero.Shared.DTOs;

public record SendFriendRequestRequest(string AddresseeEmail);

// DTO para exibir um pedido de amizade pendente
public record FriendRequestResponse(
    Guid FriendshipId,
    Guid RequesterId,
    string RequesterName,
    string RequesterEmail,
    string? ProfilePictureUrl
);

// DTO para enviar a resposta a um pedido
public record RespondToFriendRequestRequest(bool Accept);
public record UserSearchResponse(
    Guid Id,
    string Name,
    string Email,
    bool IsFriend,
    bool HasPendingRequest,
    string? ProfilePictureUrl
);

public record FriendResponse(
    Guid FriendshipId,
    Guid FriendId,
    string FriendName,
    string FriendEmail,
    string? ProfilePictureUrl
);