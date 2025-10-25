using GymHero.Domain.Enums;

namespace GymHero.Domain.Entities;

public class Friendship : BaseEntity
{
    // Quem enviou o pedido
    public Guid RequesterId { get; set; }
    public User Requester { get; set; } = null!;

    // Quem recebeu o pedido
    public Guid AddresseeId { get; set; }
    public User Addressee { get; set; } = null!;

    public FriendshipStatus Status { get; set; }
}