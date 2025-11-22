using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Application.Features.WorkoutPlans.Commands;
using GymHero.Application.Services;
using GymHero.Domain.Entities;
using GymHero.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace GymHero.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments")
            .RequireAuthorization();

        // Create payment intent for workout plan purchase
        group.MapPost("/create-intent", async (
            [FromBody] CreatePaymentIntentRequest request,
            ClaimsPrincipal user,
            IPaymentService paymentService,
            IApplicationDbContext context,
            IConfiguration configuration,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            // Check if payments are enabled
            var paymentsEnabled = configuration.GetValue<bool>("Marketplace:PaymentsEnabled");
            if (!paymentsEnabled)
            {
                return Results.BadRequest(new {
                    message = "Payments are currently disabled. This feature is coming soon!"
                });
            }

            try
            {
                var buyerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

                // Get the workout plan
                var plan = await context.WorkoutPlans
                    .Include(p => p.Owner)
                    .FirstOrDefaultAsync(p => p.Id == request.WorkoutPlanId, cancellationToken);

                if (plan == null)
                {
                    logger.LogWarning("Payment attempt for non-existent plan: {PlanId} by user {UserId}",
                        request.WorkoutPlanId, buyerId);
                    return Results.NotFound(new { message = "Workout plan not found" });
                }

                if (!plan.ForSale || !plan.Price.HasValue)
                {
                    logger.LogWarning("Payment attempt for non-sale plan: {PlanId} by user {UserId}",
                        plan.Id, buyerId);
                    return Results.BadRequest(new { message = "This plan is not for sale" });
                }

                // SECURITY: Validate price is reasonable (between R$1 and R$10,000)
                if (plan.Price.Value <= 0 || plan.Price.Value > 10000)
                {
                    logger.LogError("Invalid plan price detected: {Price} for plan {PlanId}",
                        plan.Price.Value, plan.Id);
                    return Results.BadRequest(new { message = "Invalid plan price" });
                }

                if (plan.OwnerId == buyerId)
                {
                    logger.LogWarning("User {UserId} attempted to purchase their own plan {PlanId}",
                        buyerId, plan.Id);
                    return Results.BadRequest(new { message = "You cannot purchase your own plan" });
                }

                // Convert price to cents (smallest currency unit)
                var amountInCents = (long)(plan.Price.Value * 100);

                // Create payment intent
                var metadata = new Dictionary<string, string>
                {
                    { "workout_plan_id", plan.Id.ToString() },
                    { "buyer_id", buyerId.ToString() },
                    { "seller_id", plan.OwnerId.ToString() },
                };

                var clientSecret = await paymentService.CreatePaymentIntentAsync(
                    amountInCents,
                    "brl",
                    $"Purchase of workout plan: {plan.Name}",
                    metadata
                );

                // Extract payment intent ID from client secret
                var paymentIntentId = clientSecret.Split("_secret_")[0];

                // Calculate platform fee
                var platformFeePercentage = configuration.GetValue<decimal>("Marketplace:PlatformFeePercentage");
                var minimumPlatformFee = configuration.GetValue<decimal>("Marketplace:MinimumPlatformFee");

                var platformFee = Math.Max(
                    plan.Price.Value * (platformFeePercentage / 100),
                    minimumPlatformFee
                );
                var sellerPayout = plan.Price.Value - platformFee;

                // Create pending transaction record
                var transaction = new Transaction
                {
                    BuyerId = buyerId,
                    SellerId = plan.OwnerId,
                    WorkoutPlanId = plan.Id,
                    Amount = plan.Price.Value,
                    Currency = "BRL",
                    Status = TransactionStatus.Pending,
                    Provider = PaymentProvider.Stripe,
                    StripePaymentIntentId = paymentIntentId,
                    Description = $"Purchase of workout plan: {plan.Name}",
                    PlatformFee = platformFee,
                    PlatformFeePercentage = platformFeePercentage,
                    SellerPayout = sellerPayout,
                };

                context.Transactions.Add(transaction);
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Payment intent created: {PaymentIntentId} for plan {PlanId} by user {UserId}. Amount: {Amount} BRL",
                    paymentIntentId, plan.Id, buyerId, plan.Price.Value);

                return Results.Ok(new CreatePaymentIntentResponse(
                    clientSecret,
                    paymentIntentId,
                    amountInCents,
                    "BRL"
                ));
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Stripe error creating payment intent for user {UserId}", user.FindFirstValue(ClaimTypes.NameIdentifier));
                return Results.Problem(
                    title: "Failed to create payment intent",
                    detail: "Unable to process payment. Please try again.",
                    statusCode: 500
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating payment intent for user {UserId}", user.FindFirstValue(ClaimTypes.NameIdentifier));
                return Results.Problem(
                    title: "Failed to create payment intent",
                    detail: "An error occurred. Please try again.",
                    statusCode: 500
                );
            }
        })
        .WithName("CreatePaymentIntent")
        .WithSummary("Create a Stripe payment intent for a workout plan purchase");

        // Confirm payment and complete the purchase
        group.MapPost("/confirm", async (
            [FromBody] ConfirmPaymentRequest request,
            ClaimsPrincipal user,
            IPaymentService paymentService,
            IApplicationDbContext context,
            ISender sender,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var buyerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {

                // Find the transaction
                var transaction = await context.Transactions
                    .Include(t => t.WorkoutPlan)
                    .Include(t => t.Seller)
                    .FirstOrDefaultAsync(
                        t => t.StripePaymentIntentId == request.PaymentIntentId && t.BuyerId == buyerId,
                        cancellationToken);

                if (transaction == null)
                {
                    logger.LogWarning("Payment confirmation attempted for non-existent transaction: {PaymentIntentId} by user {UserId}",
                        request.PaymentIntentId, buyerId);
                    return Results.NotFound(new { message = "Transaction not found" });
                }

                if (transaction.Status == TransactionStatus.Completed)
                {
                    logger.LogInformation("Duplicate payment confirmation attempt for transaction: {TransactionId}",
                        transaction.Id);
                    return Results.BadRequest(new { message = "Transaction already completed" });
                }

                // Verify payment status with Stripe
                var paymentStatus = await paymentService.GetPaymentStatusAsync(request.PaymentIntentId);

                if (paymentStatus == "succeeded")
                {
                    // Update transaction status
                    transaction.Status = TransactionStatus.Completed;
                    transaction.CompletedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync(cancellationToken);

                    logger.LogInformation("Payment confirmed successfully: Transaction {TransactionId}, Amount: {Amount} BRL, Buyer: {BuyerId}, Seller: {SellerId}",
                        transaction.Id, transaction.Amount, buyerId, transaction.SellerId);

                    // Clone the workout plan to the buyer's account using the existing command
                    var cloneCommand = new CloneWorkoutPlanCommand(transaction.WorkoutPlanId, buyerId);
                    await sender.Send(cloneCommand, cancellationToken);

                    return Results.Ok(new PaymentConfirmationResponse(
                        true,
                        transaction.Id,
                        "Payment successful! Workout plan added to your account."
                    ));
                }
                else if (paymentStatus == "processing")
                {
                    logger.LogInformation("Payment still processing: {TransactionId}", transaction.Id);
                    return Results.Ok(new PaymentConfirmationResponse(
                        false,
                        null,
                        "Payment is being processed. Please check back later."
                    ));
                }
                else
                {
                    transaction.Status = TransactionStatus.Failed;
                    transaction.ErrorMessage = $"Payment status: {paymentStatus}";
                    await context.SaveChangesAsync(cancellationToken);

                    logger.LogWarning("Payment failed: {TransactionId}, Status: {PaymentStatus}", transaction.Id, paymentStatus);

                    return Results.Ok(new PaymentConfirmationResponse(
                        false,
                        null,
                        "Payment failed. Please try again."
                    ));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error confirming payment for user {UserId}", buyerId);
                return Results.Problem(
                    title: "Failed to confirm payment",
                    detail: "An error occurred processing your payment. Please contact support.",
                    statusCode: 500
                );
            }
        })
        .WithName("ConfirmPayment")
        .WithSummary("Confirm a payment and complete the purchase");

        // Get user's transaction history
        group.MapGet("/transactions", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            [FromQuery] string? type, // "purchases" or "sales"
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var query = context.Transactions
                .Include(t => t.WorkoutPlan)
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Where(t => t.BuyerId == userId || t.SellerId == userId);

            if (type == "purchases")
            {
                query = query.Where(t => t.BuyerId == userId);
            }
            else if (type == "sales")
            {
                query = query.Where(t => t.SellerId == userId);
            }

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TransactionSummaryResponse(
                    t.Id,
                    t.WorkoutPlan.Name,
                    t.Amount,
                    t.Currency,
                    t.Status.ToString(),
                    t.CreatedAt,
                    t.BuyerId == userId
                ))
                .ToListAsync(cancellationToken);

            return Results.Ok(transactions);
        })
        .WithName("GetTransactions")
        .WithSummary("Get user's transaction history");

        // Get transaction details
        group.MapGet("/transactions/{transactionId:guid}", async (
            Guid transactionId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var transaction = await context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.WorkoutPlan)
                .Where(t => t.Id == transactionId && (t.BuyerId == userId || t.SellerId == userId))
                .FirstOrDefaultAsync(cancellationToken);

            if (transaction == null)
            {
                return Results.NotFound(new { message = "Transaction not found" });
            }

            var response = new TransactionResponse(
                transaction.Id,
                transaction.BuyerId,
                transaction.Buyer.Name,
                transaction.SellerId,
                transaction.Seller.Name,
                transaction.WorkoutPlanId,
                transaction.WorkoutPlan.Name,
                transaction.Amount,
                transaction.Currency,
                transaction.Status.ToString(),
                transaction.CreatedAt,
                transaction.CompletedAt,
                transaction.ErrorMessage
            );

            return Results.Ok(response);
        })
        .WithName("GetTransactionDetails")
        .WithSummary("Get transaction details by ID");

        // Refund a transaction
        group.MapPost("/transactions/{transactionId:guid}/refund", async (
            Guid transactionId,
            ClaimsPrincipal user,
            IPaymentService paymentService,
            IPayPalPaymentService paypalService,
            IApplicationDbContext context,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var transaction = await context.Transactions
                    .Include(t => t.WorkoutPlan)
                    .Include(t => t.Buyer)
                    .Include(t => t.Seller)
                    .FirstOrDefaultAsync(t => t.Id == transactionId, cancellationToken);

                if (transaction == null)
                {
                    logger.LogWarning("Refund attempted for non-existent transaction: {TransactionId} by user {UserId}",
                        transactionId, userId);
                    return Results.NotFound(new { message = "Transaction not found" });
                }

                // Only the seller can issue refunds
                if (transaction.SellerId != userId)
                {
                    logger.LogWarning("Unauthorized refund attempt: Transaction {TransactionId} by user {UserId} (not seller)",
                        transactionId, userId);
                    return Results.Forbid();
                }

                if (transaction.Status != TransactionStatus.Completed)
                {
                    return Results.BadRequest(new { message = "Only completed transactions can be refunded" });
                }

                if (transaction.Status == TransactionStatus.Refunded)
                {
                    return Results.BadRequest(new { message = "Transaction already refunded" });
                }

                bool refundSuccess = false;

                // Process refund based on payment provider
                if (transaction.Provider == PaymentProvider.Stripe)
                {
                    if (string.IsNullOrEmpty(transaction.StripePaymentIntentId))
                    {
                        return Results.BadRequest(new { message = "Cannot refund Stripe transaction without payment intent ID" });
                    }

                    refundSuccess = await paymentService.RefundPaymentAsync(transaction.StripePaymentIntentId);
                }
                else if (transaction.Provider == PaymentProvider.PayPal)
                {
                    if (string.IsNullOrEmpty(transaction.PayPalCaptureId))
                    {
                        return Results.BadRequest(new { message = "Cannot refund PayPal transaction without capture ID" });
                    }

                    refundSuccess = await paypalService.RefundCaptureAsync(transaction.PayPalCaptureId);
                }
                else
                {
                    return Results.BadRequest(new { message = "Unknown payment provider" });
                }

                if (!refundSuccess)
                {
                    return Results.Problem(
                        title: "Refund failed",
                        detail: "Unable to process refund with payment provider",
                        statusCode: 500
                    );
                }

                // Update transaction status
                transaction.Status = TransactionStatus.Refunded;
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Refund processed successfully: Transaction {TransactionId}, Amount: {Amount} BRL, Seller: {SellerId}",
                    transaction.Id, transaction.Amount, userId);

                return Results.Ok(new
                {
                    success = true,
                    message = "Refund processed successfully",
                    transactionId = transaction.Id
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing refund for transaction {TransactionId}", transactionId);
                return Results.Problem(
                    title: "Failed to process refund",
                    detail: "Unable to process refund. Please try again or contact support.",
                    statusCode: 500
                );
            }
        })
        .WithName("RefundTransaction")
        .WithSummary("Refund a completed transaction");

        // Get revenue analytics for PT
        group.MapGet("/revenue-analytics", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {

                // Get all sales for this user
                var sales = await context.Transactions
                    .Include(t => t.Buyer)
                    .Include(t => t.WorkoutPlan)
                    .Where(t => t.SellerId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync(cancellationToken);

                // Calculate metrics
                var completedSales = sales.Where(t => t.Status == TransactionStatus.Completed).ToList();
                var refundedSales = sales.Where(t => t.Status == TransactionStatus.Refunded).ToList();

                var totalRevenue = completedSales.Sum(t => t.Amount);
                var totalSales = sales.Count;
                var completedSalesCount = completedSales.Count;
                var refundedSalesCount = refundedSales.Count;
                var averageOrderValue = completedSalesCount > 0 ? totalRevenue / completedSalesCount : 0;

                // Top selling plans
                var topSellingPlans = completedSales
                    .GroupBy(t => new { t.WorkoutPlanId, t.WorkoutPlan.Name })
                    .Select(g => new TopSellingPlan(
                        g.Key.WorkoutPlanId,
                        g.Key.Name,
                        g.Count(),
                        g.Sum(t => t.Amount)
                    ))
                    .OrderByDescending(p => p.TotalRevenue)
                    .Take(5)
                    .ToList();

                // Recent sales (last 10)
                var recentSales = sales
                    .Take(10)
                    .Select(t => new RecentSale(
                        t.Id,
                        t.Buyer.Name,
                        t.WorkoutPlan.Name,
                        t.Amount,
                        t.CreatedAt,
                        t.Status.ToString()
                    ))
                    .ToList();

                // Revenue by month (last 12 months)
                var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);
                var revenueByMonth = completedSales
                    .Where(t => t.CompletedAt >= twelveMonthsAgo)
                    .GroupBy(t => new {
                        Year = t.CompletedAt!.Value.Year,
                        Month = t.CompletedAt!.Value.Month
                    })
                    .Select(g => new RevenueByPeriod(
                        g.Key.Year,
                        g.Key.Month,
                        g.Sum(t => t.Amount),
                        g.Count()
                    ))
                    .OrderBy(r => r.Year)
                    .ThenBy(r => r.Month)
                    .ToList();

                var response = new RevenueAnalyticsResponse(
                    totalRevenue,
                    totalSales,
                    completedSalesCount,
                    refundedSalesCount,
                    averageOrderValue,
                    topSellingPlans,
                    recentSales,
                    revenueByMonth
                );

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving revenue analytics for user {UserId}", userId);
                return Results.Problem(
                    title: "Failed to get revenue analytics",
                    detail: "Unable to retrieve analytics. Please try again.",
                    statusCode: 500
                );
            }
        })
        .WithName("GetRevenueAnalytics")
        .WithSummary("Get revenue analytics for PT sales");

        // Download receipt/invoice for a transaction
        group.MapGet("/transactions/{transactionId:guid}/receipt", async (
            Guid transactionId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            IReceiptService receiptService,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {

                // Verify user has access to this transaction
                var transaction = await context.Transactions
                    .Where(t => t.Id == transactionId && (t.BuyerId == userId || t.SellerId == userId))
                    .FirstOrDefaultAsync(cancellationToken);

                if (transaction == null)
                {
                    return Results.NotFound(new { message = "Transaction not found" });
                }

                if (transaction.Status != TransactionStatus.Completed)
                {
                    return Results.BadRequest(new { message = "Receipt is only available for completed transactions" });
                }

                // Generate PDF receipt
                var pdfBytes = await receiptService.GenerateReceiptAsync(transactionId, cancellationToken);

                // Return as downloadable file
                var fileName = $"Receipt-{transactionId}-{DateTime.UtcNow:yyyyMMdd}.pdf";

                logger.LogInformation("Receipt generated for transaction {TransactionId} by user {UserId}",
                    transactionId, userId);

                return Results.File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating receipt for transaction {TransactionId}", transactionId);
                return Results.Problem(
                    title: "Failed to generate receipt",
                    detail: "Unable to generate receipt. Please try again.",
                    statusCode: 500
                );
            }
        })
        .WithName("DownloadReceipt")
        .WithSummary("Download PDF receipt for a completed transaction");

        // PayPal: Create order for workout plan purchase
        group.MapPost("/paypal/create-order", async (
            [FromBody] CreatePaymentIntentRequest request,
            ClaimsPrincipal user,
            IPayPalPaymentService paypalService,
            IApplicationDbContext context,
            IConfiguration configuration,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var buyerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {

                // Get the workout plan
                var plan = await context.WorkoutPlans
                    .Include(p => p.Owner)
                    .FirstOrDefaultAsync(p => p.Id == request.WorkoutPlanId, cancellationToken);

                if (plan == null)
                {
                    return Results.NotFound(new { message = "Workout plan not found" });
                }

                if (!plan.ForSale || !plan.Price.HasValue)
                {
                    logger.LogWarning("PayPal order creation attempted for plan not for sale: {PlanId} by user {UserId}",
                        request.WorkoutPlanId, buyerId);
                    return Results.BadRequest(new { message = "This plan is not for sale" });
                }

                // Validate price
                if (plan.Price.Value <= 0 || plan.Price.Value > 10000)
                {
                    logger.LogError("Invalid PayPal order price detected: {Price} for plan {PlanId}",
                        plan.Price.Value, plan.Id);
                    return Results.BadRequest(new { message = "Invalid plan price" });
                }

                if (plan.OwnerId == buyerId)
                {
                    logger.LogWarning("User attempted to purchase their own plan via PayPal: User {UserId}, Plan {PlanId}",
                        buyerId, plan.Id);
                    return Results.BadRequest(new { message = "You cannot purchase your own plan" });
                }

                // Calculate platform fee
                var platformFeePercentage = configuration.GetValue<decimal>("Marketplace:PlatformFeePercentage");
                var minimumPlatformFee = configuration.GetValue<decimal>("Marketplace:MinimumPlatformFee");

                var platformFee = Math.Max(
                    plan.Price.Value * (platformFeePercentage / 100),
                    minimumPlatformFee
                );
                var sellerPayout = plan.Price.Value - platformFee;

                // Create PayPal order
                var returnUrl = $"{configuration["AppUrl"]}/marketplace?paypal=success";
                var cancelUrl = $"{configuration["AppUrl"]}/marketplace?paypal=cancel";

                var (orderId, approvalUrl) = await paypalService.CreateOrderAsync(
                    plan.Price.Value,
                    "BRL",
                    $"Purchase of workout plan: {plan.Name}",
                    returnUrl,
                    cancelUrl
                );

                // Create pending transaction record
                var transaction = new Transaction
                {
                    BuyerId = buyerId,
                    SellerId = plan.OwnerId,
                    WorkoutPlanId = plan.Id,
                    Amount = plan.Price.Value,
                    Currency = "BRL",
                    Status = TransactionStatus.Pending,
                    Provider = PaymentProvider.PayPal,
                    PayPalOrderId = orderId,
                    Description = $"Purchase of workout plan: {plan.Name}",
                    PlatformFee = platformFee,
                    PlatformFeePercentage = platformFeePercentage,
                    SellerPayout = sellerPayout,
                };

                context.Transactions.Add(transaction);
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("PayPal order created: {OrderId} for plan {PlanId} by user {UserId}. Amount: {Amount} BRL",
                    orderId, plan.Id, buyerId, plan.Price.Value);

                return Results.Ok(new
                {
                    orderId = orderId,
                    approvalUrl = approvalUrl,
                    transactionId = transaction.Id,
                    amount = plan.Price.Value,
                    currency = "BRL"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating PayPal order for user {UserId}", buyerId);
                return Results.Problem(
                    title: "Failed to create PayPal order",
                    detail: "Unable to create payment order. Please try again.",
                    statusCode: 500
                );
            }
        })
        .WithName("CreatePayPalOrder")
        .WithSummary("Create a PayPal order for a workout plan purchase");

        // PayPal: Capture order after user approval
        group.MapPost("/paypal/capture-order", async (
            [FromBody] CapturePayPalOrderRequest request,
            ClaimsPrincipal user,
            IPayPalPaymentService paypalService,
            IApplicationDbContext context,
            ISender sender,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var buyerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {

                // Find the transaction
                var transaction = await context.Transactions
                    .Include(t => t.WorkoutPlan)
                    .Include(t => t.Seller)
                    .FirstOrDefaultAsync(
                        t => t.PayPalOrderId == request.OrderId && t.BuyerId == buyerId,
                        cancellationToken);

                if (transaction == null)
                {
                    logger.LogWarning("PayPal capture attempted for non-existent transaction: OrderId {OrderId} by user {UserId}",
                        request.OrderId, buyerId);
                    return Results.NotFound(new { message = "Transaction not found" });
                }

                if (transaction.Status == TransactionStatus.Completed)
                {
                    logger.LogInformation("Duplicate PayPal capture attempt for transaction: {TransactionId}",
                        transaction.Id);
                    return Results.BadRequest(new { message = "Transaction already completed" });
                }

                // Capture the PayPal order
                var (captureId, status) = await paypalService.CaptureOrderAsync(request.OrderId);

                if (status == "COMPLETED")
                {
                    // Update transaction
                    transaction.Status = TransactionStatus.Completed;
                    transaction.CompletedAt = DateTime.UtcNow;
                    transaction.PayPalCaptureId = captureId;
                    await context.SaveChangesAsync(cancellationToken);

                    logger.LogInformation("PayPal payment captured successfully: Transaction {TransactionId}, CaptureId {CaptureId}, Amount: {Amount} BRL, Buyer: {BuyerId}, Seller: {SellerId}",
                        transaction.Id, captureId, transaction.Amount, buyerId, transaction.SellerId);

                    // Clone the workout plan to the buyer's account
                    var cloneCommand = new CloneWorkoutPlanCommand(transaction.WorkoutPlanId, buyerId);
                    await sender.Send(cloneCommand, cancellationToken);

                    return Results.Ok(new PaymentConfirmationResponse(
                        true,
                        transaction.Id,
                        "Payment successful! Workout plan added to your account."
                    ));
                }
                else
                {
                    transaction.Status = TransactionStatus.Failed;
                    transaction.ErrorMessage = $"PayPal capture status: {status}";
                    await context.SaveChangesAsync(cancellationToken);

                    logger.LogWarning("PayPal payment capture failed: {TransactionId}, Status: {CaptureStatus}",
                        transaction.Id, status);

                    return Results.Ok(new PaymentConfirmationResponse(
                        false,
                        null,
                        $"Payment capture failed. Status: {status}"
                    ));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error capturing PayPal order {OrderId} for user {UserId}", request.OrderId, buyerId);
                return Results.Problem(
                    title: "Failed to capture PayPal order",
                    detail: "Unable to process payment. Please try again or contact support.",
                    statusCode: 500
                );
            }
        })
        .WithName("CapturePayPalOrder")
        .WithSummary("Capture a PayPal order after user approval");

        // Stripe webhook endpoint (no authorization required - Stripe calls this)
        app.MapPost("/api/payments/webhook", async (
            HttpRequest request,
            IConfiguration configuration,
            IApplicationDbContext context,
            ISender sender,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var json = await new StreamReader(request.Body).ReadToEndAsync();
                var stripeSignature = request.Headers["Stripe-Signature"].ToString();
                var webhookSecret = configuration["Stripe:WebhookSecret"];

                if (string.IsNullOrEmpty(webhookSecret))
                {
                    logger.LogError("Stripe webhook secret is not configured");
                    return Results.BadRequest(new { error = "Webhook secret not configured" });
                }

                Event stripeEvent;
                try
                {
                    // Verify webhook signature
                    stripeEvent = EventUtility.ConstructEvent(
                        json,
                        stripeSignature,
                        webhookSecret
                    );
                }
                catch (StripeException e)
                {
                    logger.LogError(e, "Webhook signature verification failed");
                    return Results.BadRequest(new { error = "Invalid signature" });
                }

                logger.LogInformation("Received Stripe webhook event: {EventType}", stripeEvent.Type);

                // Handle the event
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        if (paymentIntent != null)
                        {
                            await HandlePaymentSucceeded(paymentIntent, context, sender, logger, cancellationToken);
                        }
                        break;

                    case "payment_intent.payment_failed":
                        var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        if (failedPaymentIntent != null)
                        {
                            await HandlePaymentFailed(failedPaymentIntent, context, logger, cancellationToken);
                        }
                        break;

                    case "payment_intent.canceled":
                        var canceledPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        if (canceledPaymentIntent != null)
                        {
                            await HandlePaymentCanceled(canceledPaymentIntent, context, logger, cancellationToken);
                        }
                        break;

                    case "charge.refunded":
                        var charge = stripeEvent.Data.Object as Charge;
                        if (charge != null)
                        {
                            await HandleChargeRefunded(charge, context, logger, cancellationToken);
                        }
                        break;

                    default:
                        logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Results.Ok(new { received = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing webhook");
                return Results.Problem(
                    title: "Webhook processing failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("StripeWebhook")
        .WithSummary("Handle Stripe webhook events")
        .AllowAnonymous(); // Webhooks don't use bearer token auth
    }

    private static async Task HandlePaymentSucceeded(
        PaymentIntent paymentIntent,
        IApplicationDbContext context,
        ISender sender,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing payment success for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

        var transaction = await context.Transactions
            .Include(t => t.WorkoutPlan)
            .FirstOrDefaultAsync(t => t.StripePaymentIntentId == paymentIntent.Id, cancellationToken);

        if (transaction == null)
        {
            logger.LogWarning("Transaction not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
            return;
        }

        if (transaction.Status == TransactionStatus.Completed)
        {
            logger.LogInformation("Transaction already completed: {TransactionId}", transaction.Id);
            return;
        }

        // Update transaction status
        transaction.Status = TransactionStatus.Completed;
        transaction.CompletedAt = DateTime.UtcNow;
        transaction.StripeChargeId = paymentIntent.LatestChargeId;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Transaction marked as completed: {TransactionId}", transaction.Id);

        // Clone workout plan to buyer's account
        try
        {
            var cloneCommand = new CloneWorkoutPlanCommand(transaction.WorkoutPlanId, transaction.BuyerId);
            await sender.Send(cloneCommand, cancellationToken);
            logger.LogInformation("Workout plan cloned for buyer: {BuyerId}", transaction.BuyerId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clone workout plan for transaction: {TransactionId}", transaction.Id);
            // Note: Transaction is still marked as completed, but plan cloning failed
            // This can be retried manually or through admin interface
        }
    }

    private static async Task HandlePaymentFailed(
        PaymentIntent paymentIntent,
        IApplicationDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing payment failure for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

        var transaction = await context.Transactions
            .FirstOrDefaultAsync(t => t.StripePaymentIntentId == paymentIntent.Id, cancellationToken);

        if (transaction == null)
        {
            logger.LogWarning("Transaction not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
            return;
        }

        transaction.Status = TransactionStatus.Failed;
        transaction.ErrorMessage = paymentIntent.LastPaymentError?.Message ?? "Payment failed";
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Transaction marked as failed: {TransactionId}", transaction.Id);
    }

    private static async Task HandlePaymentCanceled(
        PaymentIntent paymentIntent,
        IApplicationDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing payment cancellation for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);

        var transaction = await context.Transactions
            .FirstOrDefaultAsync(t => t.StripePaymentIntentId == paymentIntent.Id, cancellationToken);

        if (transaction == null)
        {
            logger.LogWarning("Transaction not found for PaymentIntent: {PaymentIntentId}", paymentIntent.Id);
            return;
        }

        transaction.Status = TransactionStatus.Cancelled;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Transaction marked as cancelled: {TransactionId}", transaction.Id);
    }

    private static async Task HandleChargeRefunded(
        Charge charge,
        IApplicationDbContext context,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing charge refund for Charge: {ChargeId}", charge.Id);

        var transaction = await context.Transactions
            .FirstOrDefaultAsync(t => t.StripeChargeId == charge.Id || t.StripePaymentIntentId == charge.PaymentIntentId, cancellationToken);

        if (transaction == null)
        {
            logger.LogWarning("Transaction not found for Charge: {ChargeId}", charge.Id);
            return;
        }

        if (transaction.Status == TransactionStatus.Refunded)
        {
            logger.LogInformation("Transaction already refunded: {TransactionId}", transaction.Id);
            return;
        }

        transaction.Status = TransactionStatus.Refunded;
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Transaction marked as refunded: {TransactionId}", transaction.Id);
    }
}
