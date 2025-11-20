using GymHero.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace GymHero.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly string _apiKey;

    public StripePaymentService(IConfiguration configuration)
    {
        _apiKey = configuration["Stripe:SecretKey"]
            ?? throw new ArgumentNullException("Stripe:SecretKey configuration is missing");

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
            var service = new PaymentIntentService();
            var paymentIntent = await service.ConfirmAsync(paymentIntentId);

            return paymentIntent.Status == "succeeded";
        }
        catch (StripeException)
        {
            return false;
        }
    }

    public async Task<string> GetPaymentStatusAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            return paymentIntent.Status;
        }
        catch (StripeException)
        {
            return "error";
        }
    }

    public async Task<bool> RefundPaymentAsync(string paymentIntentId, long? amount = null)
    {
        try
        {
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

            return refund.Status == "succeeded" || refund.Status == "pending";
        }
        catch (StripeException)
        {
            return false;
        }
    }
}
