# NS Backend API

ASP.NET Core 8 REST API for the Massage Booking application.

## Tech Stack
- ASP.NET Core 8 (Clean Architecture)
- Entity Framework Core + PostgreSQL
- Hangfire (background jobs)
- Redis (caching)
- JWT Authentication

## Quick Start

```bash
# Using Docker
docker compose up

# Or manually
docker compose up -d postgres redis
cd src/MassageBooking.Api
dotnet run
```

API runs on `http://localhost:5000`
Swagger UI at `http://localhost:5000/swagger`

## Environment Variables

See `appsettings.json` or the root `README.md` for full list.
