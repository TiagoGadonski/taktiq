namespace GymHero.Domain.Entities;

public class ProgressMetric : BaseEntity
{
    // Chave estrangeira para o dono da métrica
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public string Type { get; set; } = string.Empty; // Ex: "Peso", "Medida", "PR"
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty; // Ex: "kg", "cm", "reps"
    public DateTime Date { get; set; }
}