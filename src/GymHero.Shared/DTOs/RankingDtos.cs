namespace GymHero.Shared.DTOs;

public record RankingUserResponse(
    int Rank,
    Guid UserId,
    string UserName,
    int Score // Neste caso, a pontuação será o número de treinos
);