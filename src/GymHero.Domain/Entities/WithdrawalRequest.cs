namespace GymHero.Domain.Entities;

/// <summary>
/// Represents a withdrawal request from a trainer to receive their earnings
/// </summary>
public class WithdrawalRequest : BaseEntity
{
    /// <summary>
    /// ID of the trainer requesting the withdrawal
    /// </summary>
    public Guid TrainerId { get; set; }

    /// <summary>
    /// Navigation property to the trainer
    /// </summary>
    public User Trainer { get; set; } = null!;

    /// <summary>
    /// Amount requested for withdrawal (in local currency)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., "BRL", "USD")
    /// </summary>
    public string Currency { get; set; } = "BRL";

    /// <summary>
    /// Status of the withdrawal request
    /// </summary>
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;

    /// <summary>
    /// Payment method for payout
    /// </summary>
    public PayoutMethod Method { get; set; } = PayoutMethod.StripeConnect;

    /// <summary>
    /// Stripe payout ID (after payout is processed)
    /// </summary>
    public string? StripePayoutId { get; set; }

    /// <summary>
    /// When the withdrawal was requested
    /// </summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the withdrawal was processed (null if pending or rejected)
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// ID of the admin who processed the withdrawal
    /// </summary>
    public Guid? ProcessedByAdminId { get; set; }

    /// <summary>
    /// Navigation property to the admin who processed the withdrawal
    /// </summary>
    public User? ProcessedByAdmin { get; set; }

    /// <summary>
    /// Notes from the trainer about the withdrawal
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Rejection reason (if status is Rejected)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Error message if the withdrawal failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Status of a withdrawal request
/// </summary>
public enum WithdrawalStatus
{
    /// <summary>
    /// Withdrawal is pending (awaiting admin approval)
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Withdrawal was approved and is being processed
    /// </summary>
    Processing = 1,

    /// <summary>
    /// Withdrawal was completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Withdrawal was rejected by admin
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// Withdrawal failed during processing
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Withdrawal was cancelled by the trainer
    /// </summary>
    Cancelled = 5
}

/// <summary>
/// Payout method for withdrawal
/// </summary>
public enum PayoutMethod
{
    /// <summary>
    /// Stripe Connect account payout
    /// </summary>
    StripeConnect = 0,

    /// <summary>
    /// Bank transfer (manual processing)
    /// </summary>
    BankTransfer = 1,

    /// <summary>
    /// PayPal payout
    /// </summary>
    PayPal = 2
}
