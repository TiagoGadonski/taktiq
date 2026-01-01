namespace GymHero.Domain.Entities;

public class StudentAssessment : BaseEntity
{
    // Relacionamentos
    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public Guid TrainerId { get; set; }
    public User Trainer { get; set; } = null!;

    // Metadados
    public string AssessmentType { get; set; } = "Postural"; // "Postural", "Physical", "Neuromotor", "Custom"
    public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true; // Apenas a mais recente ativa por tipo

    // ===== AVALIAÇÃO POSTURAL =====
    // Valores: "None", "Mild", "Moderate", "Severe"
    public string? ForwardHead { get; set; }          // Cabeça anteriorizada
    public string? RoundedShoulders { get; set; }     // Ombros protusos
    public string? AnteriorPelvicTilt { get; set; }   // Inclinação pélvica anterior
    public string? PosteriorPelvicTilt { get; set; }  // Inclinação pélvica posterior
    public string? KneeValgus { get; set; }           // Joelhos valgos (para dentro)
    public string? KneeVarus { get; set; }            // Joelhos varos (para fora)
    public string? FlatFeet { get; set; }             // Pés planos
    public string? Scoliosis { get; set; }            // Escoliose

    // ===== AVALIAÇÃO FÍSICA =====
    public double? BodyFatPercentage { get; set; }    // Percentual de gordura corporal
    public double? MuscleMass { get; set; }           // Massa muscular (kg)
    public double? FlexibilityScore { get; set; }     // Score de flexibilidade (0-10)
    public double? StrengthScore { get; set; }        // Score de força (0-10)
    public double? CardioScore { get; set; }          // Score cardiovascular (0-10)

    // ===== CAMPOS CUSTOMIZADOS =====
    // JSON: [{"fieldName": "Equilíbrio", "fieldValue": "Bom", "fieldType": "text"}]
    public string? CustomFields { get; set; }

    // ===== OBSERVAÇÕES DO PT (PRIVADAS) =====
    public string? TrainerNotes { get; set; }

    // ===== RECOMENDAÇÕES AUTOMÁTICAS =====
    // Gerado pela AI ou pelo sistema baseado nos desvios identificados
    public string? Recommendations { get; set; }
}
