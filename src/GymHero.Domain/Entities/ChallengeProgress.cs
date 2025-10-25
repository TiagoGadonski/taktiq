namespace GymHero.Domain.Entities;

public class ChallengeProgress : BaseEntity
{
    public Guid ChallengeId { get; set; }
    public Challenge Challenge { get; set; } = null!;

    // Adicionamos o ID do participante (o aluno)
    public Guid ParticipantId { get; set; }
    public User Participant { get; set; } = null!;

    public double CurrentValue { get; set; }
    public DateTime LastUpdate { get; set; }
}