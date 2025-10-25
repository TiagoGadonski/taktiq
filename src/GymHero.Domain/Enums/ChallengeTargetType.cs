namespace GymHero.Domain.Enums;

/// <summary>
/// Define quem pode participar do desafio
/// </summary>
public enum ChallengeTargetType
{
    /// <summary>
    /// Desafio para todos os usuários da plataforma
    /// </summary>
    AllUsers,

    /// <summary>
    /// Desafio exclusivo para Personal Trainers
    /// </summary>
    AllTrainers,

    /// <summary>
    /// Desafio para usuários específicos (comportamento atual)
    /// </summary>
    SpecificUsers
}
