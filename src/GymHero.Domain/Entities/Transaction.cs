namespace GymHero.Domain.Entities;

/// <summary>
/// Represents a financial transaction (purchase) in the system
/// </summary>
public class Transaction : BaseEntity
{
    /// <summary>
    /// ID of the user who made the purchase (buyer)
    /// </summary>
    public Guid BuyerId { get; set; }

    /// <summary>
    /// Navigation property to the buyer
    /// </summary>
    public User Buyer { get; set; } = null!;

    /// <summary>
    /// ID of the user who receives the payment (seller)
    /// </summary>
    public Guid SellerId { get; set; }

    /// <summary>
    /// Navigation property to the seller
    /// </summary>
    public User Seller { get; set; } = null!;

    /// <summary>
    /// ID of the workout plan that was purchased
    /// </summary>
    public Guid WorkoutPlanId { get; set; }

    /// <summary>
    /// Navigation property to the workout plan
    /// </summary>
    public WorkoutPlan WorkoutPlan { get; set; } = null!;

    /// <summary>
    /// Amount paid in the transaction (in local currency)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., "BRL", "USD")
    /// </summary>
    public string Currency { get; set; } = "BRL";

    /// <summary>
    /// Status of the transaction
    /// </summary>
    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Payment provider used for this transaction
    /// </summary>
    public PaymentProvider Provider { get; set; } = PaymentProvider.Stripe;

    /// <summary>
    /// Stripe Payment Intent ID
    /// </summary>
    public string? StripePaymentIntentId { get; set; }

    /// <summary>
    /// Stripe Charge ID
    /// </summary>
    public string? StripeChargeId { get; set; }

    /// <summary>
    /// PayPal Order ID
    /// </summary>
    public string? PayPalOrderId { get; set; }

    /// <summary>
    /// PayPal Capture ID (after order is captured)
    /// </summary>
    public string? PayPalCaptureId { get; set; }

    /// <summary>
    /// Description of the transaction
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// When the payment was completed (null if pending or failed)
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Error message if the transaction failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional metadata stored as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Platform fee amount (percentage of transaction)
    /// </summary>
    public decimal PlatformFee { get; set; } = 0;

    /// <summary>
    /// Platform fee percentage applied
    /// </summary>
    public decimal PlatformFeePercentage { get; set; } = 0;

    /// <summary>
    /// Net amount paid to seller (Amount - PlatformFee)
    /// </summary>
    public decimal SellerPayout { get; set; } = 0;
}

/// <summary>
/// Status of a transaction
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Payment is pending (awaiting confirmation)
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment was completed successfully
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Payment failed
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Payment was refunded
    /// </summary>
    Refunded = 3,

    /// <summary>
    /// Payment was cancelled before completion
    /// </summary>
    Cancelled = 4
}

/// <summary>
/// Payment provider used for transaction
/// </summary>
public enum PaymentProvider
{
    /// <summary>
    /// Stripe payment gateway
    /// </summary>
    Stripe = 0,

    /// <summary>
    /// PayPal payment gateway
    /// </summary>
    PayPal = 1
}
