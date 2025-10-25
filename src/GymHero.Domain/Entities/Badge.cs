namespace GymHero.Domain.Entities;

public class Badge : BaseEntity
{
    // Chave estrangeira para o dono da medalha
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    // Um código único para identificar a regra que gerou a medalha
    public string Code { get; set; } = string.Empty; // Ex: "FIRST_WEEK_COMPLETE", "NEW_PR"
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EarnedAt { get; set; }
}