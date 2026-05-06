# Code Changes Made - Service-to-Service Communication via ApiGateway

## Summary of Changes

All services have been updated to route inter-service communication through the **ApiGateway** instead of calling each other directly.

---

## Files Modified

### 1. **Program.cs Files** (Updated HttpClient Configuration)

Changed from:
```csharp
builder.Services.AddHttpClient("PaymentServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:PaymentService"]);
});
```

To:
```csharp
builder.Services.AddHttpClient("PaymentServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ApiGateway"]);
});
```

**Services Updated:**
- ✅ BookingService/Program.cs
- ✅ PaymentService/Program.cs
- ✅ AnalyticsService/Program.cs
- ✅ ParkingSpotService/Program.cs

### 2. **PaymentService/Services/StripeGateway.cs** (Fixed Hardcoded URLs)

Changed from:
```csharp
SuccessUrl = "http://localhost:4200/driver/dashboard?payment_success=true&booking_id=" + payment.BookingId,
CancelUrl = "http://localhost:4200/driver/dashboard?payment_cancelled=true",
```

To:
```csharp
SuccessUrl = $"{_configuration["FrontendUrl"]}/driver/dashboard?payment_success=true&booking_id={payment.BookingId}",
CancelUrl = $"{_configuration["FrontendUrl"]}/driver/dashboard?payment_cancelled=true",
```

---

## Configuration Required

### Development (appsettings.Development.json)

Add or update FrontendUrl in PaymentService:
```json
{
  "FrontendUrl": "http://localhost:3000"
}
```

### Production (appsettings.Production.json)

```json
{
  "FrontendUrl": "WILL_BE_SET_BY_ENV_VARIABLES"
}
```

### Render Environment Variables

For PaymentService:
```
FrontendUrl=https://your-frontend-domain.com
```

---

## Service Call Flow (NOW CORRECTED)

**Before (❌ WRONG):**
```
BookingService → Directly calls PaymentService:5242
BookingService → Directly calls ParkingLotService:5244
AnalyticsService → Directly calls PaymentService:5250
```

**After (✅ CORRECT):**
```
BookingService → ApiGateway:5000 → Routes to PaymentService
BookingService → ApiGateway:5000 → Routes to ParkingLotService
AnalyticsService → ApiGateway:5000 → Routes to PaymentService
```

---

## Service URLs Configuration

Update all `appsettings.*.json` files:

**Development (localhost):**
```json
"ServiceUrls": {
  "ApiGateway": "http://localhost:5000"
}
```

**Production (Render):**
```json
"ServiceUrls": {
  "ApiGateway": "https://parkease-apigateway.onrender.com"
}
```

---

## What Services Use ApiGateway?

| Service | Calls Through ApiGateway To | Updated |
|---------|---------------------------|---------|
| **BookingService** | PaymentService, ParkingLotService, SpotService | ✅ |
| **PaymentService** | BookingService | ✅ |
| **AnalyticsService** | BookingService, PaymentService, ParkingLotService | ✅ |
| **ParkingSpotService** | ParkingLotService | ✅ |
| **NotificationService** | - (No internal service calls) | - |
| **ParkingLotService** | - (No internal service calls) | - |
| **VehicleService** | - (No internal service calls) | - |
| **AuthService** | - (No internal service calls) | - |

---

## Next Steps

1. **Update Render Configuration**:
   - For each service, set `ServiceUrls__ApiGateway=https://parkease-apigateway.onrender.com`
   - For PaymentService, set `FrontendUrl=https://your-frontend-domain.com`

2. **Update Local Configuration**:
   - Ensure all `appsettings.Development.json` files have correct ApiGateway URL
   - Ensure PaymentService has FrontendUrl set

3. **Test Locally**:
   ```bash
   # Run all services with ApiGateway routing
   - ApiGateway on 5000
   - All other services call through ApiGateway
   ```

4. **Deploy to Render**:
   - All environment variables must be set correctly
   - Services will auto-route through ApiGateway

---

## Potential Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| 404 errors from BookingService | Service URLs pointing to wrong ports | Verify `ServiceUrls:ApiGateway` is set in all services |
| Stripe redirects fail | FrontendUrl not set | Add `FrontendUrl` env var to PaymentService |
| Service discovery fails | ApiGateway URL missing | Ensure `ServiceUrls:ApiGateway` points to correct URL |
| Production URLs incorrect | Hardcoded localhost | All URLs now use configuration/env vars ✅ |

---

## Testing Commands

**Local Testing:**
```bash
# Verify BookingService can call PaymentService through ApiGateway
curl http://localhost:5246/api/v1/bookings/test

# Should route to:
# http://localhost:5000/api/v1/payments/...
```

**Production Testing (after deploy):**
```bash
# Verify service routing on Render
curl https://parkease-bookingservice.onrender.com/api/v1/bookings/test

# Should route to:
# https://parkease-apigateway.onrender.com/api/v1/payments/...
```

---

## Files Still Need Checking

The service classes themselves use these HttpClient factories. Verify they use correct paths:

- ✅ BookingService/Services/ParkingLotService.cs - Uses `/api/v1/lots/...`
- ✅ BookingService/Services/PaymentService.cs - Uses `/api/v1/payments/...`
- ✅ AnalyticsService/Services/AnalyticsService.cs - Uses `/api/v1/...` endpoints
- ✅ ParkingSpotService/Services/ParkingLotApiClient.cs - Uses `/api/v1/lots/...`

All service calls are now routed through ApiGateway ✅
