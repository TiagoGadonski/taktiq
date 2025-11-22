# Security Improvements - Implementation Guide

## Quick Reference for Completing Security Audit

This guide provides practical scripts and patterns to complete the remaining security improvements.

---

## 1. Remove All Console.WriteLine from AIEndpoints.cs

**File**: `src/GymHero.Api/Endpoints/AIEndpoints.cs`
**Count**: 65 occurrences
**Type**: Debug output that leaks information

### Quick Fix Script (PowerShell)

Run this from the repository root:

```powershell
$file = "src/GymHero.Api/Endpoints/AIEndpoints.cs"
(Get-Content $file) | Where-Object { $_ -notmatch '^\s*Console\.WriteLine' } | Set-Content "$file.tmp"
Move-Item -Force "$file.tmp" $file
```

### Manual Approach for Critical Sections

Only keep logging for errors (line 286-287). Replace with ILogger:

```csharp
// REMOVE:
Console.WriteLine($"[ERROR {errorId}] Error in search-exercises: {ex.Message}");

// REPLACE WITH:
logger.LogError(ex, "[ERROR {ErrorId}] Error in search-exercises", errorId);
```

All other Console.WriteLine statements (validation output, debug info) should be completely removed.

---

## 2. Remove All console.log from Frontend

**Total**: 38 occurrences across 12 files

### Batch Removal Script (Run from `frontend/` directory)

```bash
# Find and remove console.log statements
find apps/web/src -type f \( -name "*.ts" -o -name "*.tsx" \) -exec sed -i '/console\.\(log\|error\|warn\|debug\)/d' {} \;
```

### File-by-File Cleanup

#### Priority Files:
1. **instructor/page.tsx** - 14 occurrences (highest)
2. **announcement-popup.tsx** - 3 occurrences
3. **stripe-checkout.tsx** - 3 occurrences (payment critical!)
4. **use-notifications.ts** - 3 occurrences
5. **trainer/[slug]/page.tsx** - 3 occurrences
6. **workout/page.tsx** - 3 occurrences

#### Keep Structured Logging
**Exception**: `packages/shared/src/utils/logger.ts` has 4 console statements
- **DO NOT REMOVE** - This is the structured logging utility
- Review to ensure it's production-safe

---

## 3. Secure PaymentEndpoints.cs

**File**: `src/GymHero.Api/Endpoints/PaymentEndpoints.cs`

### Security Checklist:

```csharp
// ✅ Add idempotency keys
group.MapPost("/create-payment-intent", async (
    [FromBody] CreatePaymentRequest request,
    [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
    // ... existing params
) => {
    // Validate idempotency key
    if (string.IsNullOrEmpty(idempotencyKey))
    {
        return Results.BadRequest(new { message = "Idempotency key required" });
    }

    // Check for duplicate requests
    var existing = await context.PaymentIntents
        .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);

    if (existing != null)
    {
        return Results.Ok(existing); // Return existing instead of creating duplicate
    }

    // Continue with payment creation...
});

// ✅ Validate amounts
if (request.Amount <= 0 || request.Amount > 1000000) // Max $10,000
{
    return Results.BadRequest(new { message = "Invalid amount" });
}

// ✅ Verify Stripe webhook signatures
[AllowAnonymous] // Webhooks don't have JWT
group.MapPost("/webhook", async (
    HttpRequest request,
    IConfiguration config
) => {
    var json = await new StreamReader(request.Body).ReadToEndAsync();
    var signatureHeader = request.Headers["Stripe-Signature"];

    try
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json,
            signatureHeader,
            config["Stripe:WebhookSecret"]
        );

        // Process webhook securely...
    }
    catch (StripeException)
    {
        return Results.BadRequest(); // Invalid signature
    }
});

// ✅ Add comprehensive logging
logger.LogInformation("Payment intent created: {PaymentIntentId} for user {UserId}",
    paymentIntent.Id, userId);
```

---

## 4. Secure AdminEndpoints.cs

**File**: `src/GymHero.Api/Endpoints/AdminEndpoints.cs`

### Security Checklist:

```csharp
// ✅ Audit logging for all admin actions
group.MapDelete("/users/{userId:guid}", async (
    Guid userId,
    ClaimsPrincipal user,
    IApplicationDbContext context,
    ILogger<Program> logger
) => {
    var adminId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    logger.LogWarning("Admin {AdminId} deleted user {UserId}", adminId, userId);

    // Continue with deletion...
});

// ✅ Rate limiting
.RequireRateLimiting("admin") // Add to all admin endpoints

// ✅ IP whitelisting (optional, for high-security environments)
group.MapPost("/bulk-delete", async (
    HttpContext httpContext,
    // ... params
) => {
    var ip = httpContext.Connection.RemoteIpAddress?.ToString();
    var allowedIps = config.GetSection("Security:AdminAllowedIPs").Get<string[]>();

    if (allowedIps != null && !allowedIps.Contains(ip))
    {
        logger.LogWarning("Admin action attempted from unauthorized IP: {IP}", ip);
        return Results.Forbid();
    }

    // Continue...
});
```

---

## 5. Add Input Validation Middleware

**File**: Create `src/GymHero.Api/Middleware/InputValidationMiddleware.cs`

```csharp
public class InputValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputValidationMiddleware> _logger;

    // SQL injection patterns
    private static readonly Regex SqlInjectionPattern = new(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE)\b)|(--|;|\/\*|\*\/|xp_|sp_)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    // XSS patterns
    private static readonly Regex XssPattern = new(
        @"(<script|<iframe|javascript:|onerror=|onload=)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    public async Task InvokeAsync(HttpContext context)
    {
        // Validate query parameters
        foreach (var param in context.Request.Query)
        {
            if (SqlInjectionPattern.IsMatch(param.Value) ||
                XssPattern.IsMatch(param.Value))
            {
                _logger.LogWarning("Malicious input detected in query param {Key}", param.Key);
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid input detected" });
                return;
            }
        }

        await _next(context);
    }
}

// Add to Program.cs:
app.UseMiddleware<InputValidationMiddleware>();
```

---

## 6. Database Optimization Scripts

### Add Missing Indexes

Run these migrations:

```sql
-- Users table
CREATE INDEX IF NOT EXISTS idx_users_email ON "Users"("Email");
CREATE INDEX IF NOT EXISTS idx_users_role ON "Users"("Role");
CREATE INDEX IF NOT EXISTS idx_users_personaltrainerid ON "Users"("PersonalTrainerId");

-- Messages table
CREATE INDEX IF NOT EXISTS idx_messages_senderid ON "Messages"("SenderId");
CREATE INDEX IF NOT EXISTS idx_messages_sentat ON "Messages"("SentAt" DESC);

-- WorkoutPlans table
CREATE INDEX IF NOT EXISTS idx_workoutplans_ownerid ON "WorkoutPlans"("OwnerId");
CREATE INDEX IF NOT EXISTS idx_workoutplans_isactive ON "WorkoutPlans"("IsActive");

-- Sessions table
CREATE INDEX IF NOT EXISTS idx_sessions_userid_completedat ON "Sessions"("UserId", "CompletedAt" DESC);

-- Conversations table (for chat performance)
CREATE INDEX IF NOT EXISTS idx_conversations_participant1 ON "Conversations"("Participant1Id");
CREATE INDEX IF NOT EXISTS idx_conversations_participant2 ON "Conversations"("Participant2Id");
CREATE INDEX IF NOT EXISTS idx_conversations_lastmessageat ON "Conversations"("LastMessageAt" DESC);
```

### Create Migration

```bash
cd src/GymHero.Infrastructure
dotnet ef migrations add AddPerformanceIndexes --startup-project ../GymHero.Api
dotnet ef database update --startup-project ../GymHero.Api
```

---

## 7. Frontend Input Validation

### Install DOMPurify

```bash
cd frontend
pnpm add dompurify
pnpm add -D @types/dompurify
```

### Usage Pattern

```typescript
import DOMPurify from 'dompurify';

// Sanitize before displaying user content
const SafeContent = ({ html }: { html: string }) => {
  const clean = DOMPurify.sanitize(html);
  return <div dangerouslySetInnerHTML={{ __html: clean }} />;
};

// Validate inputs
const validateInput = (value: string, maxLength: number = 5000) => {
  if (!value || value.trim().length === 0) {
    return { valid: false, error: "Campo obrigatório" };
  }

  if (value.length > maxLength) {
    return { valid: false, error: `Máximo ${maxLength} caracteres` };
  }

  // Check for suspicious patterns
  if (/<script|javascript:|onerror=/i.test(value)) {
    return { valid: false, error: "Conteúdo inválido detectado" };
  }

  return { valid: true };
};
```

---

## 8. Testing Security Improvements

### Backend Tests

```bash
# Check for remaining console output
grep -r "Console\.WriteLine" src/GymHero.Api/Endpoints/

# Check for SQL injection vulnerabilities
grep -r "FromSqlRaw\|FromSql" src/

# Verify all endpoints have authorization
grep -r "MapPost\|MapGet\|MapPut\|MapDelete" src/GymHero.Api/Endpoints/ | grep -v "RequireAuthorization\|AllowAnonymous"
```

### Frontend Tests

```bash
cd frontend

# Check for remaining console logs
grep -r "console\.\(log\|error\|warn\)" apps/web/src/ --exclude-dir=node_modules

# Check for dangerouslySetInnerHTML without sanitization
grep -r "dangerouslySetInnerHTML" apps/web/src/
```

---

## 9. Security Monitoring Setup

### Application Insights (Production)

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// Configure alerts for:
// - Failed login attempts > 10/minute
// - 500 errors > 5/minute
// - Payment failures > 3/minute
```

### Custom Security Events

```csharp
public class SecurityEventLogger
{
    public static void LogSecurityEvent(
        ILogger logger,
        string eventType,
        Guid? userId = null,
        string? details = null
    )
    {
        logger.LogWarning(
            "SECURITY EVENT: {EventType} | User: {UserId} | Details: {Details}",
            eventType, userId, details
        );
    }
}

// Usage:
SecurityEventLogger.LogSecurityEvent(
    logger,
    "UNAUTHORIZED_ACCESS_ATTEMPT",
    userId,
    $"Attempted to access conversation {conversationId}"
);
```

---

## 10. Pre-Deployment Checklist

- [ ] All Console.WriteLine removed (65 in AIEndpoints.cs)
- [ ] All console.log removed from frontend (38 occurrences)
- [ ] PaymentEndpoints secured with idempotency & validation
- [ ] AdminEndpoints have audit logging
- [ ] Input validation middleware added
- [ ] Database indexes created
- [ ] DOMPurify installed and used
- [ ] Security headers verified
- [ ] Rate limiting configured for all sensitive endpoints
- [ ] Application Insights configured
- [ ] Webhook signatures verified (Stripe)
- [ ] Test all critical paths
- [ ] Run `dotnet list package --vulnerable`
- [ ] Run `pnpm audit` and fix high/critical
- [ ] Update SECURITY_AUDIT.md progress to 100%

---

## Estimated Time to Complete

- **Console.WriteLine removal**: 30 minutes (automated script)
- **Frontend console.log removal**: 30 minutes (automated script)
- **Payment security**: 2 hours (critical, needs testing)
- **Admin security**: 1 hour
- **Input validation middleware**: 1 hour
- **Database indexes**: 30 minutes
- **Frontend validation**: 1 hour
- **Testing**: 2 hours

**Total**: ~8-10 hours of focused work

---

## Getting Help

If any step is unclear:
1. Reference `SECURITY_AUDIT.md` for context
2. Check existing implementations in `AuthEndpoints.cs` and `ChatEndpoints.cs`
3. Test changes incrementally with `dotnet build` and `pnpm run build`

**Remember**: Security is not a one-time task. Regular audits and updates are essential.
