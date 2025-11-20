using GymHero.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Payments;
using PayPalHttp;

namespace GymHero.Infrastructure.Services;

public class PayPalPaymentService : IPayPalPaymentService
{
    private readonly PayPalHttpClient _client;
    private readonly ILogger<PayPalPaymentService> _logger;

    public PayPalPaymentService(IConfiguration configuration, ILogger<PayPalPaymentService> logger)
    {
        _logger = logger;

        // Get PayPal configuration
        var clientId = configuration["PayPal:ClientId"];
        var clientSecret = configuration["PayPal:ClientSecret"];
        var mode = configuration["PayPal:Mode"]; // "sandbox" or "live"

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("PayPal configuration is missing. Please set PayPal:ClientId and PayPal:ClientSecret");
        }

        // Create PayPal environment
        PayPalEnvironment environment = mode?.ToLower() == "live"
            ? new LiveEnvironment(clientId, clientSecret)
            : new SandboxEnvironment(clientId, clientSecret);

        _client = new PayPalHttpClient(environment);
    }

    public async Task<(string orderId, string approvalUrl)> CreateOrderAsync(
        decimal amount,
        string currency,
        string description,
        string returnUrl,
        string cancelUrl)
    {
        try
        {
            var orderRequest = new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE",
                ApplicationContext = new ApplicationContext
                {
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl,
                    BrandName = "GymHero",
                    UserAction = "PAY_NOW",
                    ShippingPreference = "NO_SHIPPING"
                },
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        Description = description,
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = currency.ToUpper(),
                            Value = amount.ToString("F2")
                        }
                    }
                }
            };

            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(orderRequest);

            var response = await _client.Execute(request);
            var result = response.Result<Order>();

            // Get approval URL from links
            var approvalUrl = result.Links.FirstOrDefault(l => l.Rel == "approve")?.Href
                ?? throw new InvalidOperationException("PayPal approval URL not found");

            _logger.LogInformation("PayPal order created: {OrderId}", result.Id);

            return (result.Id, approvalUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create PayPal order");
            throw;
        }
    }

    public async Task<(string captureId, string status)> CaptureOrderAsync(string orderId)
    {
        try
        {
            var request = new OrdersCaptureRequest(orderId);
            request.Prefer("return=representation");
            request.RequestBody(new OrderActionRequest());

            var response = await _client.Execute(request);
            var result = response.Result<Order>();

            var capture = result.PurchaseUnits[0].Payments.Captures[0];

            _logger.LogInformation("PayPal order captured: {OrderId}, Capture: {CaptureId}, Status: {Status}",
                orderId, capture.Id, capture.Status);

            return (capture.Id, capture.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture PayPal order: {OrderId}", orderId);
            throw;
        }
    }

    public async Task<string> GetOrderStatusAsync(string orderId)
    {
        try
        {
            var request = new OrdersGetRequest(orderId);
            var response = await _client.Execute(request);
            var result = response.Result<Order>();

            return result.Status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get PayPal order status: {OrderId}", orderId);
            throw;
        }
    }

    public async Task<bool> RefundCaptureAsync(string captureId, decimal? amount = null)
    {
        try
        {
            var request = new CapturesRefundRequest(captureId);

            if (amount.HasValue)
            {
                request.RequestBody(new PayPalCheckoutSdk.Payments.RefundRequest
                {
                    Amount = new PayPalCheckoutSdk.Payments.Money
                    {
                        CurrencyCode = "BRL", // Note: Should be parameterized if supporting multiple currencies
                        Value = amount.Value.ToString("F2")
                    }
                });
            }
            else
            {
                request.RequestBody(new PayPalCheckoutSdk.Payments.RefundRequest());
            }

            var response = await _client.Execute(request);
            var result = response.Result<PayPalCheckoutSdk.Payments.Refund>();

            _logger.LogInformation("PayPal refund processed: Capture {CaptureId}, Refund {RefundId}, Status: {Status}",
                captureId, result.Id, result.Status);

            return result.Status == "COMPLETED";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refund PayPal capture: {CaptureId}", captureId);
            throw;
        }
    }
}
