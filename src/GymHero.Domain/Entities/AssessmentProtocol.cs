using GymHero.Shared.Enums;

namespace GymHero.Domain.Entities;

/// <summary>
/// Represents a standardized physical assessment protocol template
/// </summary>
public class AssessmentProtocol : BaseEntity
{
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AssessmentProtocolType ProtocolType { get; set; }
    public string Category { get; set; } = string.Empty; // "Cardiovascular", "Strength", "Flexibility", etc.

    // Protocol Details
    public string? Instructions { get; set; }  // Step-by-step instructions
    public string? Equipment { get; set; }     // Required equipment
    public int? DurationMinutes { get; set; } // Estimated duration

    // Measurement Fields
    // JSON array defining what measurements are needed
    // Example: [{"fieldName": "Distance", "fieldType": "number", "unit": "meters"}]
    public string MeasurementFields { get; set; } = "[]";

    // Normative Data
    // JSON with age/gender-based classification tables
    // Example: {"male": {"20-29": {"excellent": ">2800", "good": "2400-2800"...}}}
    public string? NormativeData { get; set; }

    // Calculation Formula
    // Formula to calculate the final score/result
    // Example: "VO2Max = (Distance - 504.9) / 44.73" for Cooper Test
    public string? CalculationFormula { get; set; }

    // Visibility
    public bool IsPublic { get; set; } = true;  // Public protocols vs custom trainer protocols
    public Guid? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Relations
    public ICollection<AssessmentResult> Results { get; set; } = new List<AssessmentResult>();
}
