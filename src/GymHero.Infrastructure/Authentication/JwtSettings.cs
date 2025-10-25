namespace GymHero.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "JwtSettings"; // Para facilitar a leitura no DI
    public string Secret { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public int ExpiryMinutes { get; init; }
}