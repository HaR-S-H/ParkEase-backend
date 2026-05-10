# ParkEase Render Deployment - Environment Variables Reference

## DO NOT COMMIT THIS FILE - Use Render Dashboard or GitHub Secrets Instead

---

## AuthService Environment Variables

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Database
ConnectionStrings__DefaultConnection=Host=ep-silent-breeze-an8vmzxy-pooler.c-6.us-east-1.aws.neon.tech;Database=AuthDB;Username=neondb_owner;Password=YOUR_NEON_PASSWORD;SSL Mode=VerifyFull;Channel Binding=Require;

# JWT
Jwt__Key=your_jwt_key_minimum_64_characters_long_for_security

# AWS
AWS__AccessKey=YOUR_AWS_ACCESS_KEY
AWS__SecretKey=YOUR_AWS_SECRET_KEY

# Email (Gmail)
Mail__Smtp__Username=anantgita108@gmail.com
Mail__Smtp__Password=YOUR_GMAIL_APP_PASSWORD

# CORS
Cors__AllowedOrigins__0=https://your-frontend-domain.com
```

---

## BookingService Environment Variables

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Database
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Port=5432;Database=BookingDB;Username=YOUR_USER;Password=YOUR_PASSWORD;

# Service URLs
ServiceUrls__AuthService=https://parkease-authservice.onrender.com
ServiceUrls__PaymentService=https://parkease-paymentservice.onrender.com
ServiceUrls__NotificationService=https://parkease-notificationservice.onrender.com

# JWT
Jwt__Key=your_jwt_key_minimum_64_characters_long_for_security
```

---

## PaymentService Environment Variables

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Database
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Port=5432;Database=PaymentDB;Username=YOUR_USER;Password=YOUR_PASSWORD;

# Payment Gateways
Gateway__StripeApiKey=sk_live_YOUR_STRIPE_SECRET_KEY
Gateway__RazorpayKey=YOUR_RAZORPAY_KEY
Gateway__RazorpaySecret=YOUR_RAZORPAY_SECRET

# Service URLs (through ApiGateway)
ServiceUrls__ApiGateway=https://parkease-apigateway.onrender.com

# Frontend URL (for Stripe redirect)
FrontendUrl=https://your-frontend-domain.com
```

---

## NotificationService Environment Variables

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Database
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Port=5432;Database=NotificationDB;Username=YOUR_USER;Password=YOUR_PASSWORD;

# Email
Mail__Smtp__Username=anantgita108@gmail.com
Mail__Smtp__Password=YOUR_GMAIL_APP_PASSWORD

# Twilio
Twilio__AccountSid=ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
Twilio__AuthToken=YOUR_TWILIO_AUTH_TOKEN
Twilio__FromNumber=+1XXXXXXXXXX

# RabbitMQ (if using message queue)
RabbitMQ__Url=amqps://YOUR_RABBITMQ_USER:PASSWORD@HOST/VHOST
RabbitMQ__Queues__EmailVerification=auth.email.verification
```

---

## ParkingLotService Environment Variables

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Database
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Port=5432;Database=ParkingLotDB;Username=YOUR_USER;Password=YOUR_PASSWORD;

# AWS (for image uploads)
AWS__AccessKey=YOUR_AWS_ACCESS_KEY
AWS__SecretKey=YOUR_AWS_SECRET_KEY
AWS__Region=us-east-1
AWS__S3__BucketName=amz-harsh-parkease

# RabbitMQ
RabbitMQ__Url=amqps://YOUR_RABBITMQ_USER:PASSWORD@HOST/VHOST
RabbitMQ__Queues__ParkingLotImageUpload=parkinglot.image.upload
```

---

## ParkingSpotService Environment Variables

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Database
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Port=5432;Database=ParkingSpotDB;Username=YOUR_USER;Password=YOUR_PASSWORD;

# Service URLs
ServiceUrls__ParkingLotService=https://parkease-parkinglotservice.onrender.com

# JWT
Jwt__Key=your_jwt_key_minimum_64_characters_long_for_security
```

---

## VehicleService Environment Variables

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Database
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Port=5432;Database=VehicleDB;Username=YOUR_USER;Password=YOUR_PASSWORD;

# JWT
Jwt__Key=your_jwt_key_minimum_64_characters_long_for_security
```

---

## AnalyticsService Environment Variables

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Database
ConnectionStrings__DefaultConnection=Host=YOUR_POSTGRES_HOST;Port=5432;Database=AnalyticsDB;Username=YOUR_USER;Password=YOUR_PASSWORD;
```

---

## ApiGateway Environment Variables

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:10000

# Service Routes
Services__AuthService=https://parkease-authservice.onrender.com
Services__BookingService=https://parkease-bookingservice.onrender.com
Services__PaymentService=https://parkease-paymentservice.onrender.com
Services__NotificationService=https://parkease-notificationservice.onrender.com
Services__ParkingLotService=https://parkease-parkinglotservice.onrender.com
Services__ParkingSpotService=https://parkease-parkingspotservice.onrender.com
Services__VehicleService=https://parkease-vehicleservice.onrender.com
Services__AnalyticsService=https://parkease-analyticsservice.onrender.com
```

---

## How to Set These in Render

1. **Go to Web Service on Render Dashboard**
2. **Click "Environment"**
3. **Paste each variable (Key=Value)**
4. **Click "Save"**
5. **Redeploy service**: **Settings** → **Redeploy**

---

## Common Secrets Location

| Secret | Where to Get |
|--------|-------------|
| `STRIPE_API_KEY` | Stripe Dashboard → API Keys |
| `RAZORPAY_KEY` | Razorpay Dashboard → Settings → API Keys |
| `AWS_ACCESS_KEY` | AWS IAM → Users → Security Credentials |
| `TWILIO_ACCOUNT_SID` | Twilio Dashboard → Account Info |
| `GMAIL_APP_PASSWORD` | Google Account → Security → App Passwords |
| `JWT_KEY` | Generate: `openssl rand -base64 64` |
| `NEON_PASSWORD` | Neon.tech Dashboard → Database → Connection String |

---

## To Generate a Secure JWT Key

```bash
# On Windows PowerShell
[System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString())) + [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString()))

# On Linux/Mac
openssl rand -base64 64
```

---

## Notes

- **Never hardcode secrets** in appsettings files
- **Use Render environment variables** for production secrets
- **GitHub Secrets** can auto-populate Render env vars during CI/CD
- **For local testing**, keep appsettings.Development.json with dev credentials (gitignored)
- **Connection strings** should include timeout: `Connection Timeout=30;`
