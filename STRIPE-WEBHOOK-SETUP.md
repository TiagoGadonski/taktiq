# Stripe Webhook Setup Guide

This guide explains how to set up Stripe webhooks for the GymHero marketplace payment system.

## Overview

Webhooks allow Stripe to notify your application about payment events asynchronously. This is essential for production use to ensure payment confirmations are processed reliably even if the user closes their browser.

## Webhook Endpoint

The application exposes a webhook endpoint at:
```
POST /api/payments/webhook
```

This endpoint handles the following events:
- `payment_intent.succeeded` - Payment completed successfully
- `payment_intent.payment_failed` - Payment failed
- `payment_intent.canceled` - Payment was canceled

## Setup Steps

### 1. Get Your Webhook Secret from Stripe

1. Go to the [Stripe Dashboard](https://dashboard.stripe.com/)
2. Navigate to **Developers** → **Webhooks**
3. Click **Add endpoint**
4. Enter your webhook URL:
   - For local testing: Use Stripe CLI (see below)
   - For production: `https://your-domain.com/api/payments/webhook`
5. Select events to listen for:
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
   - `payment_intent.canceled`
6. Click **Add endpoint**
7. Copy the **Signing secret** (starts with `whsec_`)

### 2. Configure Your Application

Add the webhook secret to your configuration:

**Option A: Environment Variables (Recommended for Production)**
```bash
export Stripe__WebhookSecret="whsec_your_webhook_secret_here"
```

**Option B: appsettings.json (Development Only)**
```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_secret_key_here",
    "WebhookSecret": "whsec_your_webhook_secret_here"
  }
}
```

**Option C: Azure App Service Configuration**
1. Go to Azure Portal
2. Navigate to your App Service
3. Go to **Configuration** → **Application settings**
4. Add new setting:
   - Name: `Stripe:WebhookSecret`
   - Value: `whsec_your_webhook_secret_here`

### 3. Local Testing with Stripe CLI

For local development, use the Stripe CLI to forward webhook events:

1. **Install Stripe CLI:**
   - Windows: Download from [Stripe CLI](https://stripe.com/docs/stripe-cli)
   - macOS: `brew install stripe/stripe-cli/stripe`
   - Linux: Download from [Stripe CLI](https://stripe.com/docs/stripe-cli)

2. **Login to Stripe:**
   ```bash
   stripe login
   ```

3. **Forward webhooks to your local server:**
   ```bash
   stripe listen --forward-to localhost:5000/api/payments/webhook
   ```

4. **Copy the webhook signing secret** displayed by the CLI (starts with `whsec_`)

5. **Add the secret to your configuration** (see step 2 above)

6. **Trigger test events:**
   ```bash
   stripe trigger payment_intent.succeeded
   stripe trigger payment_intent.payment_failed
   ```

## How It Works

### Payment Flow with Webhooks

1. **Frontend**: User clicks "Comprar" on a paid workout plan
2. **Backend**: Creates a payment intent and pending transaction
3. **Frontend**: User completes payment via Stripe Elements
4. **Stripe**: Sends webhook event to `/api/payments/webhook`
5. **Backend**: Verifies webhook signature
6. **Backend**: Updates transaction status
7. **Backend**: Clones workout plan to buyer's account

### Event Handlers

**`payment_intent.succeeded`**
- Marks transaction as `Completed`
- Records charge ID
- Clones workout plan to buyer
- Logs success

**`payment_intent.payment_failed`**
- Marks transaction as `Failed`
- Records error message
- Logs failure

**`payment_intent.canceled`**
- Marks transaction as `Cancelled`
- Logs cancellation

## Security

The webhook endpoint:
- ✅ Verifies Stripe signature using `EventUtility.ConstructEvent()`
- ✅ Rejects requests with invalid signatures
- ✅ Uses HTTPS in production
- ✅ Logs all webhook events for auditing
- ✅ Does not require authentication (Stripe validates via signature)

## Monitoring

Check your application logs for webhook processing:

```
Information: Received Stripe webhook event: payment_intent.succeeded
Information: Processing payment success for PaymentIntent: pi_xxx
Information: Transaction marked as completed: {TransactionId}
Information: Workout plan cloned for buyer: {BuyerId}
```

## Troubleshooting

### Webhook signature verification failed
- Check that your webhook secret is correctly configured
- Ensure you're using the correct secret for your environment (test vs. live)
- Verify the Stripe CLI is forwarding to the correct URL

### Transaction not found for PaymentIntent
- Ensure the payment intent was created through your application
- Check that the transaction was saved to the database

### Workout plan cloning failed
- Check application logs for specific error
- Verify the workout plan still exists
- Ensure buyer has proper permissions

## Testing

Test the webhook endpoint locally:

```bash
# 1. Start your application
dotnet run --project src/GymHero.Api

# 2. In another terminal, forward webhooks
stripe listen --forward-to localhost:5000/api/payments/webhook

# 3. In a third terminal, trigger test events
stripe trigger payment_intent.succeeded
stripe trigger payment_intent.payment_failed
stripe trigger payment_intent.canceled
```

## Production Deployment

Before deploying to production:

1. ✅ Configure webhook secret in Azure App Service settings
2. ✅ Create webhook endpoint in Stripe Dashboard with production URL
3. ✅ Test webhook delivery using Stripe Dashboard "Send test webhook"
4. ✅ Monitor logs for successful webhook processing
5. ✅ Set up alerts for webhook failures

## Resources

- [Stripe Webhooks Documentation](https://stripe.com/docs/webhooks)
- [Stripe CLI Documentation](https://stripe.com/docs/stripe-cli)
- [Stripe Events Reference](https://stripe.com/docs/api/events/types)
- [Webhook Best Practices](https://stripe.com/docs/webhooks/best-practices)
