namespace GymHero.Domain.Entities;

// Esta entidade guarda a DEFINIÇÃO de uma conquista que pode ser ganha.
public class BadgeDefinition : BaseEntity
{
    public string Code { get; set; } = string.Empty; // Ex: "TEN_WORKOUTS", "FIFTY_WORKOUTS"
    public string Title { get; set; } = string.Empty; // Ex: "Maratonista de Ferro"
    public string Description { get; set; } = string.Empty; // Ex: "Completou 10 treinos."
    
    // --- A Lógica da Regra ---
    public string TriggerType { get; set; } = string.Empty; // Ex: "WORKOUTS_COMPLETED"
    public int ThresholdValue { get; set; } // O valor a ser atingido. Ex: 10
}