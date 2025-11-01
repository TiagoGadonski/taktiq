using GymHero.Domain.Enums;

namespace GymHero.Domain.Entities;

public class Challenge : BaseEntity
{
    // O ID do Personal Trainer ou Admin que criou o desafio.
    public Guid CreatorId { get; set; }
    public User Creator { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double TargetValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Nome do ícone a ser usado para este desafio (ex: "trophy", "flame", "star")
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Define o público-alvo do desafio (Todos usuários, Todos trainers, ou Usuários específicos)
    /// </summary>
    public ChallengeTargetType TargetType { get; set; } = ChallengeTargetType.SpecificUsers;

    /// <summary>
    /// Indica se este é um desafio padrão criado pelo sistema para todos os usuários
    /// </summary>
    public bool IsDefault { get; set; } = false;

    // Um desafio agora tem vários registos de progresso, um para cada participante.
    public ICollection<ChallengeProgress> Progresses { get; set; } = new List<ChallengeProgress>();
}