# AuthService

AuthService is the authentication and account-management microservice for ParkEase.

It provides:
- User registration and login
- Google login
- JWT + refresh-token auth
- Cookie-based auth token support
- Email verification
- Forgot password with temporary password via RabbitMQ
- Profile updates (basic fields, password, and profile picture) in one route

## Tech Stack

- ASP.NET Core (.NET 10)
- Entity Framework Core + PostgreSQL
- JWT Bearer Authentication
- RabbitMQ (message queue)
- MailKit (SMTP email)
- AWS S3 (profile picture storage)

## Project Structure

- `Controllers/` HTTP API endpoints
- `Services/` business logic
- `Repositories/` data access
- `Data/` EF Core DbContext
- `Messaging/` publisher, messages, and consumers
- `Models/` entities and DTOs

## Configuration

Main config files:
- `appsettings.json`
- `appsettings.Development.json`

Important sections:
- `ConnectionStrings:DefaultConnection`
- `Jwt`
- `Mail`
- `RabbitMQ`
- `AWS`

Note:
- In Development, values in `appsettings.Development.json` override `appsettings.json`.
- Prefer environment variables or secret storage for credentials in non-local environments.

## Run Locally

1. Ensure PostgreSQL is running and `ConnectionStrings:DefaultConnection` is valid.
2. Ensure RabbitMQ is reachable via `RabbitMQ:Url`.
3. Ensure SMTP credentials are valid.
4. Ensure AWS S3 credentials and bucket are valid.
5. Run AuthService:

```bash
dotnet run --project AuthService/AuthService.csproj
```

Default local URLs are configured in `Properties/launchSettings.json`.

## Message Queues

Configured under `RabbitMQ:Queues`:
- `EmailVerification` -> `auth.email.verification`
- `ProfilePictureUpload` -> `auth.profile.picture.upload`
- `ForgotPassword` -> `auth.forgot.password`

Consumers:
- `EmailVerificationConsumer`
- `ProfilePictureUploadConsumer`
- `ForgotPasswordConsumer`

## API Endpoints

Base route: `/api/v1/auth`

Public:
- `POST /register` (multipart/form-data)
- `POST /login`
- `POST /google-login`
- `POST /refresh` (body token or cookie)
- `GET /verify-email?email=...&token=...`
- `POST /resend-verification`
- `POST /forgot-password`
- `POST /validate`

Authorized:
- `POST /logout`
- `GET /profile`
- `PUT /profile` (multipart/form-data; unified account update)
- `DELETE /deactivate`

## Cookie Auth Behavior

On successful login/google-login/refresh:
- `accessToken` cookie is set (HttpOnly)
- `refreshToken` cookie is set (HttpOnly)

JWT middleware also reads `accessToken` from cookie when Authorization header is missing.

On logout:
- Refresh tokens are revoked in DB
- `accessToken` and `refreshToken` cookies are deleted

## Unified Profile Update

`PUT /api/v1/auth/profile` supports optional fields in one call:
- `fullName`
- `phone`
- `vehiclePlate`
- `profilePic` (file)
- `currentPassword`
- `newPassword`

Rules:
- Password change requires both `currentPassword` and `newPassword`.
- Profile picture upload is queued and processed asynchronously.

## Forgot Password Flow

`POST /api/v1/auth/forgot-password`
- Generates a secure random temporary password
- Updates user password hash
- Revokes active refresh tokens
- Publishes forgot-password email message to RabbitMQ
- Consumer sends temporary password by email

## Troubleshooting

- If emails are not sent, verify SMTP username/password and app-password settings.
- If profile pictures are not uploaded, verify AWS credentials, region, bucket, and permissions.
- If messages remain queued/unacked, inspect consumer logs and RabbitMQ connection settings.
- If verification links fail locally, ensure URL/HTTP/HTTPS settings match your active launch profile.
