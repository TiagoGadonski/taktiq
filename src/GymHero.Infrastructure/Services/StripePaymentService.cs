using GymHero.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace GymHero.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly string _apiKey;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        IConfiguration configuration,
        ILogger<StripePaymentService> logger)
    {
        _logger = logger;
        _apiKey = configuration["Stripe:SecretKey"]
            ?? throw new ArgumentNullException("Stripe:SecretKey configuration is missing");

        // Warn if using placeholder API key
        if (_apiKey.Contains("your_stripe") || _apiKey.Contains("placeholder"))
        {
            _logger.LogWarning("Stripe is configured with a placeholder API key. Payment processing will not work.");
        }

        StripeConfiguration.ApiKey = _apiKey;
    }

    public async Task<string> CreatePaymentIntentAsync(
        long amount,
        string currency,
        string description,
        Dictionary<string, string>? metadata = null)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = amount,
            Currency = currency.ToLower(),
            Description = description,
            Metadata = metadata,
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        return paymentIntent.ClientSecret;
    }

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId)
    {
        try
        {
            _logger.LogInformation("Confirming Stripe payment intent: {PaymentIntentId}", paymentIntentId);

            var service = new PaymentIntentService();
            var paymentIntent = await service.ConfirmAsync(paymentIntentId);

            var succeeded = paymentIntent.Status == "succeeded";
            _logger.LogInformation("Payment intent confirmation result: {PaymentIntentId}, Status: {Status}, Success: {Success}",
                paymentIntentId, paymentIntent.Status, succeeded);

            return succeeded;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error confirming payment intent: {PaymentIntentId}. Code: {ErrorCode}, Message: {ErrorMessage}",
                paymentIntentId, ex.StripeError?.Code, ex.Message);
            return false;
        }
    }

    public async Task<string> GetPaymentStatusAsync(string paymentIntentId)
    {
        try
        {
            _logger.LogInformation("Getting Stripe payment status: {PaymentIntentId}", paymentIntentId);

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            _logger.LogInformation("Payment status retrieved: {PaymentIntentId}, Status: {Status}",
                paymentIntentId, paymentIntent.Status);

            return paymentIntent.Status;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error getting payment status: {PaymentIntentId}. Code: {ErrorCode}, Message: {ErrorMessage}",
                paymentIntentId, ex.StripeError?.Code, ex.Message);
            return "error";
        }
    }

    public async Task<bool> RefundPaymentAsync(string paymentIntentId, long? amount = null)
    {
        try
        {
            _logger.LogInformation("Initiating Stripe refund: PaymentIntent: {PaymentIntentId}, Amount: {Amount}",
                paymentIntentId, amount?.ToString() ?? "full refund");

            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
            };

            if (amount.HasValue)
            {
                options.Amount = amount.Value;
            }

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            var succeeded = refund.Status == "succeeded" || refund.Status == "pending";
            _logger.LogInformation("Refund result: RefundId: {RefundId}, Status: {Status}, Success: {Success}",
                refund.Id, refund.Status, succeeded);

            return succeeded;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error processing refund: {PaymentIntentId}. Code: {ErrorCode}, Message: {ErrorMessage}",
                paymentIntentId, ex.StripeError?.Code, ex.Message);
            return false;
        }
    }
}
