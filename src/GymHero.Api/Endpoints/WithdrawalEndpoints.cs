using System.Security.Claims;
using GymHero.Application.Common.Interfaces;
using GymHero.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymHero.Api.Endpoints;

public static class WithdrawalEndpoints
{
    public static void MapWithdrawalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/withdrawals")
            .WithTags("Withdrawals")
            .RequireAuthorization();

        // Get trainer balance (available earnings)
        group.MapGet("/balance", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var trainerRole = user.FindFirstValue(ClaimTypes.Role);

            // Only trainers can check balance
            if (trainerRole != "PersonalTrainer")
            {
                return Results.Forbid();
            }

            try
            {
                // Calculate total earnings from completed transactions
                var totalEarnings = await context.Transactions
                    .Where(t => t.SellerId == trainerId && t.Status == TransactionStatus.Completed)
                    .SumAsync(t => t.SellerPayout, cancellationToken);

                // Calculate total withdrawn amount
                var totalWithdrawn = await context.WithdrawalRequests
                    .Where(w => w.TrainerId == trainerId && w.Status == WithdrawalStatus.Completed)
                    .SumAsync(w => w.Amount, cancellationToken);

                // Calculate pending withdrawals
                var pendingWithdrawals = await context.WithdrawalRequests
                    .Where(w => w.TrainerId == trainerId &&
                               (w.Status == WithdrawalStatus.Pending || w.Status == WithdrawalStatus.Processing))
                    .SumAsync(w => w.Amount, cancellationToken);

                var availableBalance = totalEarnings - totalWithdrawn - pendingWithdrawals;

                // Get transaction count
                var transactionCount = await context.Transactions
                    .Where(t => t.SellerId == trainerId && t.Status == TransactionStatus.Completed)
                    .CountAsync(cancellationToken);

                logger.LogInformation("Trainer {TrainerId} checked balance: Available {Available} BRL, Total Earnings {Total} BRL",
                    trainerId, availableBalance, totalEarnings);

                return Results.Ok(new
                {
                    availableBalance,
                    totalEarnings,
                    totalWithdrawn,
                    pendingWithdrawals,
                    transactionCount,
                    currency = "BRL"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching balance for trainer {TrainerId}", trainerId);
                return Results.Problem("Failed to fetch balance. Please try again.");
            }
        })
        .WithName("GetTrainerBalance")
        .WithSummary("Get trainer's available balance and earnings summary");

        // Request a withdrawal
        group.MapPost("/request", async (
            [FromBody] CreateWithdrawalRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var trainerRole = user.FindFirstValue(ClaimTypes.Role);

            // Only trainers can request withdrawals
            if (trainerRole != "PersonalTrainer")
            {
                return Results.Forbid();
            }

            try
            {
                // Validate amount
                if (request.Amount <= 0)
                {
                    return Results.BadRequest(new { message = "Amount must be greater than zero" });
                }

                // Get trainer info
                var trainer = await context.Users
                    .FirstOrDefaultAsync(u => u.Id == trainerId, cancellationToken);

                if (trainer == null)
                {
                    return Results.NotFound(new { message = "Trainer not found" });
                }

                // Check if trainer has Stripe account ID
                if (string.IsNullOrEmpty(trainer.StripeAccountId))
                {
                    logger.LogWarning("Trainer {TrainerId} attempted withdrawal without Stripe account", trainerId);
                    return Results.BadRequest(new {
                        message = "Please connect your Stripe account before requesting a withdrawal. Contact support for assistance."
                    });
                }

                // Calculate available balance
                var totalEarnings = await context.Transactions
                    .Where(t => t.SellerId == trainerId && t.Status == TransactionStatus.Completed)
                    .SumAsync(t => t.SellerPayout, cancellationToken);

                var totalWithdrawn = await context.WithdrawalRequests
                    .Where(w => w.TrainerId == trainerId && w.Status == WithdrawalStatus.Completed)
                    .SumAsync(w => w.Amount, cancellationToken);

                var pendingWithdrawals = await context.WithdrawalRequests
                    .Where(w => w.TrainerId == trainerId &&
                               (w.Status == WithdrawalStatus.Pending || w.Status == WithdrawalStatus.Processing))
                    .SumAsync(w => w.Amount, cancellationToken);

                var availableBalance = totalEarnings - totalWithdrawn - pendingWithdrawals;

                // Check if trainer has sufficient balance
                if (request.Amount > availableBalance)
                {
                    logger.LogWarning("Trainer {TrainerId} attempted withdrawal of {Amount} BRL but only has {Available} BRL available",
                        trainerId, request.Amount, availableBalance);
                    return Results.BadRequest(new {
                        message = $"Insufficient balance. Available: R$ {availableBalance:F2}"
                    });
                }

                // Create withdrawal request
                var withdrawal = new WithdrawalRequest
                {
                    TrainerId = trainerId,
                    Amount = request.Amount,
                    Currency = "BRL",
                    Status = WithdrawalStatus.Pending,
                    Method = PayoutMethod.StripeConnect,
                    Notes = request.Notes,
                    RequestedAt = DateTime.UtcNow
                };

                context.WithdrawalRequests.Add(withdrawal);
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Withdrawal request created: {WithdrawalId} by trainer {TrainerId} for {Amount} BRL",
                    withdrawal.Id, trainerId, request.Amount);

                return Results.Ok(new
                {
                    id = withdrawal.Id,
                    amount = withdrawal.Amount,
                    currency = withdrawal.Currency,
                    status = withdrawal.Status.ToString(),
                    requestedAt = withdrawal.RequestedAt,
                    message = "Withdrawal request submitted successfully. It will be processed within 2-3 business days."
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating withdrawal request for trainer {TrainerId}", trainerId);
                return Results.Problem("Failed to create withdrawal request. Please try again.");
            }
        })
        .WithName("RequestWithdrawal")
        .WithSummary("Request a withdrawal of available earnings");

        // Get trainer's withdrawal history
        group.MapGet("/history", async (
            ClaimsPrincipal user,
            IApplicationDbContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var trainerRole = user.FindFirstValue(ClaimTypes.Role);

            // Only trainers can view their own withdrawal history
            if (trainerRole != "PersonalTrainer")
            {
                return Results.Forbid();
            }

            var skip = (page - 1) * pageSize;

            var withdrawals = await context.WithdrawalRequests
                .Where(w => w.TrainerId == trainerId)
                .OrderByDescending(w => w.RequestedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(w => new
                {
                    id = w.Id,
                    amount = w.Amount,
                    currency = w.Currency,
                    status = w.Status.ToString(),
                    method = w.Method.ToString(),
                    requestedAt = w.RequestedAt,
                    processedAt = w.ProcessedAt,
                    notes = w.Notes,
                    rejectionReason = w.RejectionReason
                })
                .ToListAsync(cancellationToken);

            var totalCount = await context.WithdrawalRequests
                .Where(w => w.TrainerId == trainerId)
                .CountAsync(cancellationToken);

            return Results.Ok(new
            {
                withdrawals,
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        })
        .WithName("GetWithdrawalHistory")
        .WithSummary("Get trainer's withdrawal history");

        // Cancel a pending withdrawal request
        group.MapDelete("/{withdrawalId:guid}", async (
            Guid withdrawalId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var trainerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var withdrawal = await context.WithdrawalRequests
                .FirstOrDefaultAsync(w => w.Id == withdrawalId && w.TrainerId == trainerId, cancellationToken);

            if (withdrawal == null)
            {
                return Results.NotFound(new { message = "Withdrawal request not found" });
            }

            // Can only cancel pending requests
            if (withdrawal.Status != WithdrawalStatus.Pending)
            {
                logger.LogWarning("Trainer {TrainerId} attempted to cancel non-pending withdrawal {WithdrawalId} with status {Status}",
                    trainerId, withdrawalId, withdrawal.Status);
                return Results.BadRequest(new {
                    message = $"Cannot cancel withdrawal with status: {withdrawal.Status}"
                });
            }

            withdrawal.Status = WithdrawalStatus.Cancelled;
            withdrawal.ProcessedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Withdrawal {WithdrawalId} cancelled by trainer {TrainerId}",
                withdrawalId, trainerId);

            return Results.Ok(new { message = "Withdrawal request cancelled successfully" });
        })
        .WithName("CancelWithdrawal")
        .WithSummary("Cancel a pending withdrawal request");

        // ADMIN: Get all pending withdrawal requests
        group.MapGet("/admin/pending", async (
            IApplicationDbContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default) =>
        {
            var skip = (page - 1) * pageSize;

            var withdrawals = await context.WithdrawalRequests
                .Include(w => w.Trainer)
                .Where(w => w.Status == WithdrawalStatus.Pending)
                .OrderBy(w => w.RequestedAt)
                .Skip(skip)
                .Take(pageSize)
                .Select(w => new
                {
                    id = w.Id,
                    trainerId = w.TrainerId,
                    trainerName = w.Trainer.Name,
                    trainerEmail = w.Trainer.Email,
                    stripeAccountId = w.Trainer.StripeAccountId,
                    amount = w.Amount,
                    currency = w.Currency,
                    status = w.Status.ToString(),
                    method = w.Method.ToString(),
                    requestedAt = w.RequestedAt,
                    notes = w.Notes
                })
                .ToListAsync(cancellationToken);

            var totalCount = await context.WithdrawalRequests
                .Where(w => w.Status == WithdrawalStatus.Pending)
                .CountAsync(cancellationToken);

            return Results.Ok(new
            {
                withdrawals,
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        })
        .RequireAuthorization("RequireAdminRole")
        .WithName("GetPendingWithdrawals")
        .WithSummary("Get all pending withdrawal requests (Admin only)");

        // ADMIN: Approve and process a withdrawal
        group.MapPost("/admin/{withdrawalId:guid}/approve", async (
            Guid withdrawalId,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var adminId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var withdrawal = await context.WithdrawalRequests
                .Include(w => w.Trainer)
                .FirstOrDefaultAsync(w => w.Id == withdrawalId, cancellationToken);

            if (withdrawal == null)
            {
                return Results.NotFound(new { message = "Withdrawal request not found" });
            }

            if (withdrawal.Status != WithdrawalStatus.Pending)
            {
                return Results.BadRequest(new {
                    message = $"Cannot approve withdrawal with status: {withdrawal.Status}"
                });
            }

            // Update status to processing
            withdrawal.Status = WithdrawalStatus.Processing;
            withdrawal.ProcessedByAdminId = adminId;
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Admin {AdminId} approved withdrawal {WithdrawalId} for trainer {TrainerId}, Amount: {Amount} BRL",
                adminId, withdrawalId, withdrawal.TrainerId, withdrawal.Amount);

            // TODO: Integrate with Stripe Connect to actually transfer money
            // For now, we'll mark it as completed immediately
            // In production, you would:
            // 1. Call Stripe API to create a payout to the connected account
            // 2. Store the payout ID
            // 3. Mark as completed after Stripe confirms

            withdrawal.Status = WithdrawalStatus.Completed;
            withdrawal.ProcessedAt = DateTime.UtcNow;
            // withdrawal.StripePayoutId = "po_xxxxx"; // Set this after Stripe payout
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Withdrawal {WithdrawalId} completed for trainer {TrainerId}",
                withdrawalId, withdrawal.TrainerId);

            return Results.Ok(new {
                message = "Withdrawal approved and processed successfully",
                withdrawal = new
                {
                    id = withdrawal.Id,
                    trainerId = withdrawal.TrainerId,
                    trainerName = withdrawal.Trainer.Name,
                    amount = withdrawal.Amount,
                    status = withdrawal.Status.ToString(),
                    processedAt = withdrawal.ProcessedAt
                }
            });
        })
        .RequireAuthorization("RequireAdminRole")
        .WithName("ApproveWithdrawal")
        .WithSummary("Approve and process a withdrawal request (Admin only)");

        // ADMIN: Reject a withdrawal
        group.MapPost("/admin/{withdrawalId:guid}/reject", async (
            Guid withdrawalId,
            [FromBody] RejectWithdrawalRequest request,
            ClaimsPrincipal user,
            IApplicationDbContext context,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var adminId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var withdrawal = await context.WithdrawalRequests
                .Include(w => w.Trainer)
                .FirstOrDefaultAsync(w => w.Id == withdrawalId, cancellationToken);

            if (withdrawal == null)
            {
                return Results.NotFound(new { message = "Withdrawal request not found" });
            }

            if (withdrawal.Status != WithdrawalStatus.Pending)
            {
                return Results.BadRequest(new {
                    message = $"Cannot reject withdrawal with status: {withdrawal.Status}"
                });
            }

            withdrawal.Status = WithdrawalStatus.Rejected;
            withdrawal.RejectionReason = request.Reason;
            withdrawal.ProcessedByAdminId = adminId;
            withdrawal.ProcessedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Admin {AdminId} rejected withdrawal {WithdrawalId} for trainer {TrainerId}. Reason: {Reason}",
                adminId, withdrawalId, withdrawal.TrainerId, request.Reason);

            return Results.Ok(new {
                message = "Withdrawal request rejected",
                withdrawal = new
                {
                    id = withdrawal.Id,
                    trainerId = withdrawal.TrainerId,
                    trainerName = withdrawal.Trainer.Name,
                    amount = withdrawal.Amount,
                    status = withdrawal.Status.ToString(),
                    rejectionReason = withdrawal.RejectionReason,
                    processedAt = withdrawal.ProcessedAt
                }
            });
        })
        .RequireAuthorization("RequireAdminRole")
        .WithName("RejectWithdrawal")
        .WithSummary("Reject a withdrawal request (Admin only)");
    }
}

// DTOs
public record CreateWithdrawalRequest(decimal Amount, string? Notes);
public record RejectWithdrawalRequest(string Reason);
