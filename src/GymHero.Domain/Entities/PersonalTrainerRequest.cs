namespace GymHero.Domain.Entities;

public class PersonalTrainerRequest : BaseEntity
{
    public Guid TrainerId { get; set; }
    public User Trainer { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public string? Message { get; set; } // Optional message from PT

    public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

    public DateTime? RespondedAt { get; set; }

    public bool IsPending => Status == "Pending";
    public bool IsAccepted => Status == "Accepted";
    public bool IsRejected => Status == "Rejected";
}
