namespace GymHero.Application.Common.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Creates a payment intent for a workout plan purchase
    /// </summary>
    /// <param name="amount">Amount in the smallest currency unit (e.g., cents for BRL)</param>
    /// <param name="currency">Currency code (e.g., "brl", "usd")</param>
    /// <param name="description">Description of the payment</param>
    /// <param name="metadata">Additional metadata for the payment</param>
    /// <returns>The client secret for the payment intent</returns>
    Task<string> CreatePaymentIntentAsync(
        long amount,
        string currency,
        string description,
        Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Confirms a payment intent
    /// </summary>
    /// <param name="paymentIntentId">The payment intent ID</param>
    /// <returns>True if the payment was confirmed successfully</returns>
    Task<bool> ConfirmPaymentAsync(string paymentIntentId);

    /// <summary>
    /// Gets the status of a payment intent
    /// </summary>
    /// <param name="paymentIntentId">The payment intent ID</param>
    /// <returns>The status of the payment ("succeeded", "processing", "requires_payment_method", etc.)</returns>
    Task<string> GetPaymentStatusAsync(string paymentIntentId);

    /// <summary>
    /// Refunds a payment
    /// </summary>
    /// <param name="paymentIntentId">The payment intent ID</param>
    /// <param name="amount">Optional partial refund amount (null for full refund)</param>
    /// <returns>True if the refund was successful</returns>
    Task<bool> RefundPaymentAsync(string paymentIntentId, long? amount = null);
}
