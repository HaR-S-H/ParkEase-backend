# ParkEase Backend Deployment on Render

This guide covers deploying your microservices architecture on Render.com with proper configuration and environment variables.

## Architecture Overview

Your services:
- **ApiGateway** - Entry point for all requests (Port: 5000)
- **AuthService** - Authentication & Authorization (Port: 5001)
- **BookingService** - Booking management (Port: 5246)
- **NotificationService** - Email/SMS notifications (Port: 5243)
- **PaymentService** - Payment processing (Port: 5242)
- **ParkingLotService** - Parking lot management (Port: 5244)
- **ParkingSpotService** - Parking spot management (Port: 5245)
- **VehicleService** - Vehicle management (Port: 5247)
- **AnalyticsService** - Analytics (Port: 5248)

---

## Prerequisites

1. **GitHub Repository**: Push all code to GitHub (make sure appsettings.json is in .gitignore)
2. **Render Account**: Create at [render.com](https://render.com)
3. **Database**: PostgreSQL database (can use Render's PostgreSQL or Neon.tech)
4. **Environment Variables**: Gather all secrets

---

## Step 1: Create PostgreSQL Databases on Render

### Option A: Use Render PostgreSQL
1. Go to Render Dashboard → New → PostgreSQL
2. Create separate databases for each service:
   - `authdb`
   - `bookingdb`
   - `notificationdb`
   - `paymentdb`
   - `parkinglotdb`
   - `parkingspotdb`
   - `vehicledb`
   - `analyticsdb`

### Option B: Use Existing Neon Database (Recommended for AuthService)
- You already have: `Host=ep-silent-breeze-an8vmzxy-pooler.c-6.us-east-1.aws.neon.tech`
- Use this for AuthService
- Create new databases on Neon for other services, or use Render PostgreSQL

---

## Step 2: Create `appsettings.Production.json` for Each Service

Create `appsettings.Production.json` in each service folder with placeholders (DO NOT commit with real secrets):

### Example for AuthService:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "WILL_BE_SET_BY_ENV_VARIABLES"
  },
  "Jwt": {
    "Key": "WILL_BE_SET_BY_ENV_VARIABLES",
    "Issuer": "ParkEase.AuthService",
    "Audience": "ParkEase.Client"
  },
  "AWS": {
    "AccessKey": "WILL_BE_SET_BY_ENV_VARIABLES",
    "SecretKey": "WILL_BE_SET_BY_ENV_VARIABLES",
    "Region": "us-east-1",
    "S3": {
      "BucketName": "amz-harsh-parkease"
    }
  },
  "Mail": {
    "From": {
      "Email": "anantgita108@gmail.com",
      "Name": "ParkEase"
    },
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": "587",
      "Username": "WILL_BE_SET_BY_ENV_VARIABLES",
      "Password": "WILL_BE_SET_BY_ENV_VARIABLES"
    }
  }
}
```

---

## Step 3: Create Web Services on Render

### For Each Service (AuthService, BookingService, etc.):

1. **Go to Render Dashboard** → New → Web Service
2. **Connect Repository**:
   - Select your GitHub repo
   - Specify branch: `ReportService` or `main`

3. **Configuration**:
   - **Name**: `parkease-authservice` (change for each)
   - **Environment**: .NET
   - **Build Command**: `dotnet build`
   - **Start Command**: `cd AuthService && dotnet AuthService.dll`
   - **Plan**: Starter (free tier) or higher

4. **Environment Variables** → Add All:

#### For AuthService:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=ep-silent-breeze-an8vmzxy-pooler.c-6.us-east-1.aws.neon.tech;Database=AuthDB;Username=neondb_owner;Password=YOUR_NEON_PASSWORD;SSL Mode=VerifyFull;Channel Binding=Require;
Jwt__Key=YOUR_JWT_KEY_64_CHARS_MINIMUM
AWS__AccessKey=YOUR_AWS_ACCESS_KEY
AWS__SecretKey=YOUR_AWS_SECRET_KEY
Mail__Smtp__Username=YOUR_EMAIL@gmail.com
Mail__Smtp__Password=YOUR_EMAIL_APP_PASSWORD
ASPNETCORE_URLS=http://+:10000
```

#### For BookingService:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Database=BookingDB;Username=USER;Password=PASSWORD;
ServiceUrls__AuthService=https://parkease-authservice.onrender.com
ServiceUrls__PaymentService=https://parkease-paymentservice.onrender.com
ASPNETCORE_URLS=http://+:10000
```

#### For PaymentService:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Database=PaymentDB;Username=USER;Password=PASSWORD;
Gateway__StripeApiKey=YOUR_STRIPE_SECRET_KEY
Gateway__RazorpayKey=YOUR_RAZORPAY_KEY
Gateway__RazorpaySecret=YOUR_RAZORPAY_SECRET
ServiceUrls__BookingService=https://parkease-bookingservice.onrender.com
ASPNETCORE_URLS=http://+:10000
```

#### For NotificationService:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Database=NotificationDB;Username=USER;Password=PASSWORD;
Mail__Smtp__Username=YOUR_EMAIL@gmail.com
Mail__Smtp__Password=YOUR_EMAIL_APP_PASSWORD
Twilio__AccountSid=YOUR_TWILIO_SID
Twilio__AuthToken=YOUR_TWILIO_AUTH_TOKEN
RabbitMQ__Url=amqps://YOUR_RABBITMQ_USER:PASSWORD@HOST/VHOST
ASPNETCORE_URLS=http://+:10000
```

#### For ParkingLotService:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Database=ParkingLotDB;Username=USER;Password=PASSWORD;
AWS__AccessKey=YOUR_AWS_ACCESS_KEY
AWS__SecretKey=YOUR_AWS_SECRET_KEY
RabbitMQ__Url=amqps://YOUR_RABBITMQ_USER:PASSWORD@HOST/VHOST
ASPNETCORE_URLS=http://+:10000
```

#### For ParkingSpotService:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Database=ParkingSpotDB;Username=USER;Password=PASSWORD;
ServiceUrls__ParkingLotService=https://parkease-parkinglotservice.onrender.com
ASPNETCORE_URLS=http://+:10000
```

#### For VehicleService:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Database=VehicleDB;Username=USER;Password=PASSWORD;
ASPNETCORE_URLS=http://+:10000
```

#### For AnalyticsService:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Database=AnalyticsDB;Username=USER;Password=PASSWORD;
ASPNETCORE_URLS=http://+:10000
```

---

## Step 4: Update ApiGateway Configuration

The ApiGateway should route to all services. Update `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Services": {
    "AuthService": "https://parkease-authservice.onrender.com",
    "BookingService": "https://parkease-bookingservice.onrender.com",
    "PaymentService": "https://parkease-paymentservice.onrender.com",
    "NotificationService": "https://parkease-notificationservice.onrender.com",
    "ParkingLotService": "https://parkease-parkinglotservice.onrender.com",
    "ParkingSpotService": "https://parkease-parkingspotservice.onrender.com",
    "VehicleService": "https://parkease-vehicleservice.onrender.com",
    "AnalyticsService": "https://parkease-analyticsservice.onrender.com"
  },
  "ASPNETCORE_URLS": "http://+:10000"
}
```

---

## Step 5: Setup Build Configuration

### Create `render.yaml` in root (Optional but Recommended):

```yaml
services:
  - type: web
    name: parkease-authservice
    env: dotnet
    buildCommand: dotnet build
    startCommand: cd AuthService && dotnet AuthService.dll
    envVars:
      - key: ASPNETCORE_ENVIRONMENT
        value: Production
      - key: ASPNETCORE_URLS
        value: http://+:10000

  - type: web
    name: parkease-bookingservice
    env: dotnet
    buildCommand: dotnet build
    startCommand: cd BookingService && dotnet BookingService.dll
    envVars:
      - key: ASPNETCORE_ENVIRONMENT
        value: Production
      - key: ASPNETCORE_URLS
        value: http://+:10000
```

---

## Step 6: Database Migrations

After deploying services, run migrations. Use Render's PostgreSQL client or connect via:

```powershell
# For Neon
psql "postgresql://neondb_owner:PASSWORD@ep-silent-breeze-an8vmzxy-pooler.c-6.us-east-1.aws.neon.tech/AuthDB"

# Run EF Core migrations for each service
dotnet ef database update --project AuthService --configuration Release
dotnet ef database update --project BookingService --configuration Release
```

Or add migration run step in `.csproj`:
```xml
<Target Name="MigrateDatabase" AfterTargets="Build">
  <Exec Command="dotnet ef database update" />
</Target>
```

---

## Step 7: Environment Variables Checklist

Create a `.env.render` file (do NOT commit) with all values:

```
# Databases
AUTHDB_CONNECTION=Host=...;Database=AuthDB;Username=...;Password=...;
BOOKINGDB_CONNECTION=Host=...;Database=BookingDB;...
PAYMENTDB_CONNECTION=Host=...;Database=PaymentDB;...

# API Keys
STRIPE_SECRET_KEY=sk_test_...
RAZORPAY_KEY=rzp_test_...
JWT_KEY=your_64_char_minimum_key

# AWS
AWS_ACCESS_KEY=AKIA...
AWS_SECRET_KEY=...

# Email
MAIL_USERNAME=your@gmail.com
MAIL_PASSWORD=your_app_password

# Twilio
TWILIO_ACCOUNT_SID=AC...
TWILIO_AUTH_TOKEN=...

# RabbitMQ
RABBITMQ_URL=amqps://...
```

---

## Step 8: Deployment Steps

1. **Push to GitHub**:
```bash
git add .
git commit -m "Add production configuration for Render"
git push origin main
```

2. **On Render Dashboard**:
   - Click "Deploy" for each service
   - Services will auto-deploy on push

3. **Monitor Logs**:
   - Click service → Logs
   - Check for startup errors

4. **Test Service**:
```bash
curl https://parkease-authservice.onrender.com/health
curl https://parkease-apigateway.onrender.com/
```

---

## Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Port binding error | Set `ASPNETCORE_URLS=http://+:10000` env var |
| Database connection timeout | Add connection timeout: `Timeout=30` in connection string |
| Service-to-service 404 | Use full HTTPS URLs in ServiceUrls (e.g., `https://parkease-authservice.onrender.com`) |
| Secrets exposed | Use Render's Environment Variables section, never commit secrets |
| Build fails | Check `Build Command`: `dotnet build` at root level |
| Service won't start | Check logs, verify DLL path: `cd ServiceName && dotnet ServiceName.dll` |
| Database migrations fail | Run migrations locally first before deploying |

---

## Step 9: CORS & API Gateway Configuration

Update ApiGateway `Program.cs` for production:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("https://your-frontend.netlify.app", "https://your-domain.com")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
```

---

## Deployment Checklist

- [ ] All services pushed to GitHub
- [ ] `.gitignore` includes `appsettings.json` and `appsettings.*.json`
- [ ] `appsettings.Production.json` exists in each service
- [ ] PostgreSQL databases created on Render/Neon
- [ ] Each service has Web Service created on Render
- [ ] All environment variables set in Render dashboard
- [ ] Start command correct for each service: `cd ServiceName && dotnet ServiceName.dll`
- [ ] ASPNETCORE_ENVIRONMENT=Production set
- [ ] ASPNETCORE_URLS=http://+:10000 set
- [ ] ApiGateway routes to correct service URLs
- [ ] Migrations run successfully
- [ ] Test endpoints respond

---

## Post-Deployment Monitoring

1. **Health Checks**: Add `/health` endpoint to each service
2. **Logs**: Monitor Render logs for errors
3. **Uptime**: Enable Render's uptime monitoring
4. **Database**: Monitor connection pool usage
5. **Performance**: Use Application Insights or similar for metrics

---

## Useful Render Commands

```bash
# View logs
render logs service_name

# View environment variables
render env list service_name

# Redeploy
render deploy service_name
```
