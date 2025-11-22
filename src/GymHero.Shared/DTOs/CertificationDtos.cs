using System.ComponentModel.DataAnnotations;

namespace GymHero.Shared.DTOs;

/// <summary>
/// Request DTO for creating a certification
/// </summary>
public record CreateCertificationRequest(
    [Required(ErrorMessage = "O nome da certificação é obrigatório")]
    string CertificationName,

    [Required(ErrorMessage = "A organização emissora é obrigatória")]
    string IssuingOrganization,

    DateTime? DateObtained,
    DateTime? ExpiryDate,
    string? ImageUrl,
    string? CredentialId
);

/// <summary>
/// Request DTO for updating a certification
/// </summary>
public record UpdateCertificationRequest(
    [Required(ErrorMessage = "O nome da certificação é obrigatório")]
    string CertificationName,

    [Required(ErrorMessage = "A organização emissora é obrigatória")]
    string IssuingOrganization,

    DateTime? DateObtained,
    DateTime? ExpiryDate,
    string? ImageUrl,
    string? CredentialId
);

/// <summary>
/// Response DTO for certification
/// </summary>
public record CertificationResponse(
    Guid Id,
    Guid TrainerId,
    string CertificationName,
    string IssuingOrganization,
    DateTime? DateObtained,
    DateTime? ExpiryDate,
    string? ImageUrl,
    string? CredentialId,
    bool IsActive,
    DateTime CreatedAt
);
