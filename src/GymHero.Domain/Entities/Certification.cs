namespace GymHero.Domain.Entities;

/// <summary>
/// Represents a certification or credential held by a Personal Trainer
/// </summary>
public class Certification : BaseEntity
{
    /// <summary>
    /// ID of the trainer who holds this certification
    /// </summary>
    public Guid TrainerId { get; set; }

    /// <summary>
    /// Navigation property to the trainer
    /// </summary>
    public User Trainer { get; set; } = null!;

    /// <summary>
    /// Name of the certification (e.g., "NASM Certified Personal Trainer")
    /// </summary>
    public string CertificationName { get; set; } = string.Empty;

    /// <summary>
    /// Organization that issued the certification
    /// </summary>
    public string IssuingOrganization { get; set; } = string.Empty;

    /// <summary>
    /// Date when certification was obtained
    /// </summary>
    public DateTime? DateObtained { get; set; }

    /// <summary>
    /// Date when certification expires (null if no expiration)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// URL to certification image/document
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Credential ID or license number
    /// </summary>
    public string? CredentialId { get; set; }

    /// <summary>
    /// Whether this certification is currently valid
    /// </summary>
    public bool IsActive
    {
        get
        {
            if (ExpiryDate == null) return true;
            return ExpiryDate > DateTime.UtcNow;
        }
    }
}
