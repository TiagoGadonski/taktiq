namespace GymHero.Domain.Entities;

public class UserActivityLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string Action { get; set; } = string.Empty; // e.g., "Login", "CreatePlan", "UpdateProfile"
    public string? Details { get; set; } // JSON or descriptive text with additional details
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string HttpMethod { get; set; } = string.Empty; // GET, POST, PUT, DELETE
    public string Endpoint { get; set; } = string.Empty; // API endpoint path
    public int? StatusCode { get; set; } // HTTP status code (200, 404, 500, etc.)
    public long? ResponseTimeMs { get; set; } // Response time in milliseconds
    public string? ErrorMessage { get; set; } // If there was an error
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
