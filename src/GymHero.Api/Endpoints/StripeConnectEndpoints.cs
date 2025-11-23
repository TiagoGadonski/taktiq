using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Claims;
using GymHero.Application.Common.Interfaces;

namespace GymHero.Api.Endpoints;

public static class StripeConnectEndpoints
{
    public static void MapStripeConnectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stripe/connect")
            .RequireAuthorization()
            .WithTags("Stripe Connect");

        group.MapPost("/create-account", CreateConnectAccount)
            .WithName("CreateStripeConnectAccount")
            .WithDescription("Create or retrieve a Stripe Connect account and get onboarding URL");

        group.MapGet("/refresh-url", RefreshOnboardingUrl)
            .WithName("RefreshStripeOnboardingUrl")
            .WithDescription("Get a new onboarding URL for an existing Stripe Connect account");

        group.MapGet("/status", GetAccountStatus)
            .WithName("GetStripeConnectStatus")
            .WithDescription("Check the status of the trainer's Stripe Connect account");

        group.MapPost("/disconnect", DisconnectAccount)
            .WithName("DisconnectStripeAccount")
            .WithDescription("Disconnect the trainer's Stripe Connect account");
    }

    private static async Task<IResult> CreateConnectAccount(
        ClaimsPrincipal user,
        IApplicationDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dbUser = await context.Users.FindAsync(new object[] { userId }, cancellationToken);

        if (dbUser == null) return Results.NotFound(new { message = "User not found" });

        if (dbUser.Role != "PersonalTrainer")
        {
            return Results.BadRequest(new { message = "Only personal trainers can connect Stripe accounts" });
        }

        var stripeSecretKey = configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(stripeSecretKey))
        {
            return Results.Problem("Stripe is not configured on the server");
        }

        StripeConfiguration.ApiKey = stripeSecretKey;

        // Create or retrieve Stripe Connect account
        string accountId;

        if (!string.IsNullOrEmpty(dbUser.StripeAccountId))
        {
            accountId = dbUser.StripeAccountId;
        }
        else
        {
            var accountOptions = new AccountCreateOptions
            {
                Type = "express",
                Email = dbUser.Email,
                Capabilities = new AccountCapabilitiesOptions
                {
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                },
                BusinessType = "individual",
                Country = "BR", // Brazil
            };

            var accountService = new AccountService();
            var account = await accountService.CreateAsync(accountOptions);

            dbUser.StripeAccountId = account.Id;
            await context.SaveChangesAsync(cancellationToken);

            accountId = account.Id;
        }

        // Create account link for onboarding
        var appUrl = configuration["AppUrl"] ?? "https://localhost:5001";
        var accountLinkOptions = new AccountLinkCreateOptions
        {
            Account = accountId,
            RefreshUrl = $"{appUrl}/stripe-connect?refresh=true",
            ReturnUrl = $"{appUrl}/earnings",
            Type = "account_onboarding",
        };

        var accountLinkService = new AccountLinkService();
        var accountLink = await accountLinkService.CreateAsync(accountLinkOptions);

        return Results.Ok(new { url = accountLink.Url });
    }

    private static async Task<IResult> RefreshOnboardingUrl(
        ClaimsPrincipal user,
        IApplicationDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dbUser = await context.Users.FindAsync(new object[] { userId }, cancellationToken);

        if (dbUser == null) return Results.NotFound(new { message = "User not found" });
        if (string.IsNullOrEmpty(dbUser.StripeAccountId))
            return Results.BadRequest(new { message = "No Stripe account connected" });

        var stripeSecretKey = configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(stripeSecretKey))
        {
            return Results.Problem("Stripe is not configured on the server");
        }

        StripeConfiguration.ApiKey = stripeSecretKey;

        var appUrl = configuration["AppUrl"] ?? "https://localhost:5001";
        var accountLinkOptions = new AccountLinkCreateOptions
        {
            Account = dbUser.StripeAccountId,
            RefreshUrl = $"{appUrl}/stripe-connect?refresh=true",
            ReturnUrl = $"{appUrl}/earnings",
            Type = "account_onboarding",
        };

        var accountLinkService = new AccountLinkService();
        var accountLink = await accountLinkService.CreateAsync(accountLinkOptions);

        return Results.Ok(new { url = accountLink.Url });
    }

    private static async Task<IResult> GetAccountStatus(
        ClaimsPrincipal user,
        IApplicationDbContext context,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dbUser = await context.Users.FindAsync(new object[] { userId }, cancellationToken);

        if (dbUser == null) return Results.NotFound(new { message = "User not found" });

        if (string.IsNullOrEmpty(dbUser.StripeAccountId))
        {
            return Results.Ok(new
            {
                connected = false,
                chargesEnabled = false,
                payoutsEnabled = false,
                detailsSubmitted = false
            });
        }

        var stripeSecretKey = configuration["Stripe:SecretKey"];
        if (string.IsNullOrEmpty(stripeSecretKey))
        {
            return Results.Problem("Stripe is not configured on the server");
        }

        StripeConfiguration.ApiKey = stripeSecretKey;

        var accountService = new AccountService();
        var account = await accountService.GetAsync(dbUser.StripeAccountId);

        return Results.Ok(new
        {
            connected = true,
            accountId = account.Id,
            chargesEnabled = account.ChargesEnabled,
            payoutsEnabled = account.PayoutsEnabled,
            detailsSubmitted = account.DetailsSubmitted,
            requirements = account.Requirements?.CurrentlyDue,
            pendingVerification = account.Requirements?.PendingVerification
        });
    }

    private static async Task<IResult> DisconnectAccount(
        ClaimsPrincipal user,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dbUser = await context.Users.FindAsync(new object[] { userId }, cancellationToken);

        if (dbUser == null) return Results.NotFound(new { message = "User not found" });

        if (string.IsNullOrEmpty(dbUser.StripeAccountId))
        {
            return Results.BadRequest(new { message = "No Stripe account connected" });
        }

        // Check if there are pending withdrawals
        var hasPendingWithdrawals = await context.WithdrawalRequests
            .AnyAsync(w => w.TrainerId == userId &&
                          (w.Status == Domain.Entities.WithdrawalStatus.Pending ||
                           w.Status == Domain.Entities.WithdrawalStatus.Processing),
                      cancellationToken);

        if (hasPendingWithdrawals)
        {
            return Results.BadRequest(new { message = "Cannot disconnect account with pending withdrawals" });
        }

        // Remove the Stripe account ID
        dbUser.StripeAccountId = null;
        await context.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { message = "Stripe account disconnected successfully" });
    }
}
