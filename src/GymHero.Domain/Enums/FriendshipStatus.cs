namespace GymHero.Domain.Enums;

public enum FriendshipStatus
{
    Pending,   // Pedido enviado, a aguardar resposta
    Accepted,  // Pedido aceite, são amigos
    Declined,  // Pedido recusado
    Blocked    // Um dos utilizadores bloqueou o outro
}