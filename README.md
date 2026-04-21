
# UC3 - Parking Spot Management

This README documents UC3 only: parking spot management in `ParkingSpotService`.

## Scope

UC3 covers the following spot operations:

- Add a single parking spot
- Add spots in bulk
- Get a spot by id
- Get spots by lot
- Get available spots in a lot
- Filter spots by type and lot
- Count available spots in a lot
- Occupy a spot
- Release a spot
- Update a spot
- Delete a spot

## Authorization

Manager-only access is enforced for these actions:

- Add spot
- Add bulk spots
- Delete spot

The service uses JWT bearer authentication and expects the role claim to be `MANAGER` for those endpoints.

## API Routes

Base route:

- `/api/v1/spots`

Endpoints:

- `POST /api/v1/spots` to add one spot
- `POST /api/v1/spots/bulk` to add multiple spots
- `GET /api/v1/spots/{spotId}` to get a spot by id
- `GET /api/v1/spots/lot/{lotId}` to get all spots for a lot
- `GET /api/v1/spots/lot/{lotId}/available` to get available spots
- `GET /api/v1/spots/lot/{lotId}/type/{spotType}` to filter by type
- `GET /api/v1/spots/lot/{lotId}/count` to count available spots
- `PUT /api/v1/spots/{spotId}/occupy` to mark a spot occupied
- `PUT /api/v1/spots/{spotId}/release` to mark a spot available
- `PUT /api/v1/spots/{spotId}` to update a spot
- `DELETE /api/v1/spots/{spotId}` to delete a spot

## Local Run

Run the service from the repository root:

```bash
dotnet run --project ParkingSpotService
```

For Swagger UI, run in Development mode:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project ParkingSpotService
```

## Required Configuration

`ParkingSpotService` expects:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Key`
- `Jwt:Issuer`
- `Jwt:Audience`

## Notes

- Swagger is enabled only in Development.
- The JWT role claim is read from the authenticated token, so the token must be issued by AuthService with the same signing key and issuer/audience values.
