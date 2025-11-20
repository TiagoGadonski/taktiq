namespace GymHero.Shared.DTOs;

/// <summary>
/// Request to create a payment intent
/// </summary>
public record CreatePaymentIntentRequest(
    Guid WorkoutPlanId
);

/// <summary>
/// Response with payment intent client secret
/// </summary>
public record CreatePaymentIntentResponse(
    string ClientSecret,
    string PaymentIntentId,
    long Amount,
    string Currency
);

/// <summary>
/// Request to confirm a payment
/// </summary>
public record ConfirmPaymentRequest(
    string PaymentIntentId,
    Guid WorkoutPlanId
);

/// <summary>
/// Request to capture a PayPal order
/// </summary>
public record CapturePayPalOrderRequest(
    string OrderId
);

/// <summary>
/// Response for payment confirmation
/// </summary>
public record PaymentConfirmationResponse(
    bool Success,
    Guid? TransactionId,
    string Message
);

/// <summary>
/// Transaction history response
/// </summary>
public record TransactionResponse(
    Guid Id,
    Guid BuyerId,
    string BuyerName,
    Guid SellerId,
    string SellerName,
    Guid WorkoutPlanId,
    string WorkoutPlanName,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? ErrorMessage
);

/// <summary>
/// Transaction summary for lists
/// </summary>
public record TransactionSummaryResponse(
    Guid Id,
    string WorkoutPlanName,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt,
    bool IsBuyer // True if the current user is the buyer, false if seller
);

/// <summary>
/// Revenue analytics for PT sales
/// </summary>
public record RevenueAnalyticsResponse(
    decimal TotalRevenue,
    int TotalSales,
    int CompletedSales,
    int RefundedSales,
    decimal AverageOrderValue,
    List<TopSellingPlan> TopSellingPlans,
    List<RecentSale> RecentSales,
    List<RevenueByPeriod> RevenueByMonth
);

/// <summary>
/// Top selling workout plan
/// </summary>
public record TopSellingPlan(
    Guid PlanId,
    string PlanName,
    int SalesCount,
    decimal TotalRevenue
);

/// <summary>
/// Recent sale information
/// </summary>
public record RecentSale(
    Guid TransactionId,
    string BuyerName,
    string WorkoutPlanName,
    decimal Amount,
    DateTime CreatedAt,
    string Status
);

/// <summary>
/// Revenue grouped by time period
/// </summary>
public record RevenueByPeriod(
    int Year,
    int Month,
    decimal Revenue,
    int SalesCount
);
