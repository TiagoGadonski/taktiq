namespace GymHero.Application.Common.Interfaces;

public interface IPayPalPaymentService
{
    /// <summary>
    /// Creates a PayPal order for a workout plan purchase
    /// </summary>
    /// <param name="amount">Amount in decimal (e.g., 100.00 for BRL 100)</param>
    /// <param name="currency">Currency code (e.g., "BRL", "USD")</param>
    /// <param name="description">Description of the payment</param>
    /// <param name="returnUrl">URL to return to after approval</param>
    /// <param name="cancelUrl">URL to return to if cancelled</param>
    /// <returns>The order ID and approval URL</returns>
    Task<(string orderId, string approvalUrl)> CreateOrderAsync(
        decimal amount,
        string currency,
        string description,
        string returnUrl,
        string cancelUrl);

    /// <summary>
    /// Captures a PayPal order after user approval
    /// </summary>
    /// <param name="orderId">The PayPal order ID</param>
    /// <returns>The capture ID and status</returns>
    Task<(string captureId, string status)> CaptureOrderAsync(string orderId);

    /// <summary>
    /// Gets the status of a PayPal order
    /// </summary>
    /// <param name="orderId">The PayPal order ID</param>
    /// <returns>The status of the order ("CREATED", "APPROVED", "COMPLETED", etc.)</returns>
    Task<string> GetOrderStatusAsync(string orderId);

    /// <summary>
    /// Refunds a captured PayPal payment
    /// </summary>
    /// <param name="captureId">The PayPal capture ID</param>
    /// <param name="amount">Optional partial refund amount (null for full refund)</param>
    /// <returns>True if the refund was successful</returns>
    Task<bool> RefundCaptureAsync(string captureId, decimal? amount = null);
}
