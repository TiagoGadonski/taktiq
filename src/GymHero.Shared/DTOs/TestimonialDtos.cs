using System.ComponentModel.DataAnnotations;

namespace GymHero.Shared.DTOs;

/// <summary>
/// Request DTO for creating a testimonial
/// </summary>
public record CreateTestimonialRequest(
    [Required(ErrorMessage = "O ID do personal trainer é obrigatório")]
    Guid TrainerId,

    [Required(ErrorMessage = "O conteúdo é obrigatório")]
    [StringLength(1000, ErrorMessage = "O depoimento deve ter no máximo 1000 caracteres")]
    string Content,

    [Range(1, 5, ErrorMessage = "A avaliação deve ser entre 1 e 5 estrelas")]
    int Rating,

    string? BeforePhotoUrl,
    string? AfterPhotoUrl,
    string? TransformationDetails,
    string? TrainingDuration
);

/// <summary>
/// Request DTO for updating a testimonial
/// </summary>
public record UpdateTestimonialRequest(
    [Required(ErrorMessage = "O conteúdo é obrigatório")]
    [StringLength(1000, ErrorMessage = "O depoimento deve ter no máximo 1000 caracteres")]
    string Content,

    [Range(1, 5, ErrorMessage = "A avaliação deve ser entre 1 e 5 estrelas")]
    int Rating,

    string? BeforePhotoUrl,
    string? AfterPhotoUrl,
    string? TransformationDetails,
    string? TrainingDuration
);

/// <summary>
/// Response DTO for testimonial
/// </summary>
public record TestimonialResponse(
    Guid Id,
    Guid TrainerId,
    string TrainerName,
    Guid StudentId,
    string StudentName,
    string? StudentProfilePictureUrl,
    string Content,
    int Rating,
    DateTime SubmittedAt,
    bool IsApproved,
    string? BeforePhotoUrl,
    string? AfterPhotoUrl,
    string? TransformationDetails,
    string? TrainingDuration
);
