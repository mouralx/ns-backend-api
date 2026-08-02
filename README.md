# Massage Booking — Local Development Guide

This guide walks any developer through running the entire stack locally on their PC.

## Prerequisites

| Tool | Version | Install |
|------|---------|---------|
| **Git** | latest | [git-scm.com](https://git-scm.com) |
| **Docker Desktop** | latest | [docker.com](https://www.docker.com/products/docker-desktop/) |
| **Node.js** | 20+ | [nodejs.org](https://nodejs.org) (LTS recommended) |
| **.NET SDK** | 8.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) |

> **You do NOT need** PostgreSQL or Redis installed locally — Docker handles that.

## Quick Start (Docker Compose)

```bash
# 1. Clone all three repos
git clone https://github.com/mouralx/ns-backend-api.git
git clone https://github.com/mouralx/ns-frontend-mobile.git
git clone https://github.com/mouralx/ns-frontend-backoffice.git

# 2. Start everything with one command
cd ns-backend-api
docker compose up
```

That's it. On first run, Docker will:
- Pull PostgreSQL 16 and Redis 7 images
- Build the API, backoffice, and mobile Docker images
- Run database migrations automatically
- Start all 5 services

**First run takes ~2-3 minutes.** Subsequent starts take ~10 seconds.

### What's Running

| Service | URL | What It Is |
|---------|-----|------------|
| **API** | http://localhost:5000 | Backend REST API |
| **Swagger** | http://localhost:5000/swagger | API documentation UI |
| **Backoffice** | http://localhost:5173 | Admin/Therapist web panel |
| **Mobile Web** | http://localhost:8080 | Mobile app (web version) |
| **PostgreSQL** | localhost:5432 | Database |
| **Redis** | localhost:6379 | Cache |
| **Hangfire** | http://localhost:5000/hangfire | Background job dashboard |

### Common Docker Commands

```bash
docker compose up              # Start all services (foreground)
docker compose up -d           # Start all services (background)
docker compose down            # Stop all services
docker compose down -v         # Stop and DELETE all data (fresh start)
docker compose logs -f api     # Follow API logs
docker compose logs -f backoffice  # Follow backoffice logs
docker compose ps              # Show running services
docker compose restart api     # Restart just the API
```

## Manual Setup (Without Docker)

If you prefer to run services individually:

### Step 1: Start Database & Cache

```bash
docker compose up -d postgres redis
```

This starts just PostgreSQL (port 5432) and Redis (port 6379).

### Step 2: Run the Backend API

```bash
cd ns-backend-api/src/MassageBooking.Api

# Restore packages
dotnet restore

# Run the API
dotnet run
```

The API starts on http://localhost:5000. Migrations run automatically on startup.

### Step 3: Run the Backoffice

```bash
cd ns-frontend-backoffice

# Install dependencies
npm install

# Set the API URL
export VITE_API_URL=http://localhost:5000

# Start dev server
npm run dev
```

Opens on http://localhost:5173.

### Step 4: Run the Mobile App

```bash
cd ns-frontend-mobile

# Install dependencies
npm install

# Set the API URL
export EXPO_PUBLIC_API_URL=http://localhost:5000

# Start Expo
npm start
```

Then:
- Press **w** to open in web browser (http://localhost:8080)
- Press **i** for iOS simulator (macOS only)
- Press **a** for Android emulator
- Scan QR code with Expo Go on your phone

## Testing the Integration

### 1. Create Your First User

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@massage.com",
    "phone": "+1234567890",
    "name": "Admin User",
    "password": "password123",
    "role": "admin"
  }'
```

### 2. Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@massage.com",
    "password": "password123"
  }'
```

You'll get back:
```json
{
  "access_token": "eyJ...",
  "refresh_token": "eyJ...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "user": {
    "id": "...",
    "email": "admin@massage.com",
    "name": "Admin User",
    "role": "admin",
    "is_active": true
  }
}
```

### 3. Open the Backoffice

Go to http://localhost:5173, login with `admin@massage.com` / `password123`.

### 4. Open the Mobile App

Go to http://localhost:8080 (or scan QR with Expo Go), login with the same credentials.

## Architecture

```
┌──────────────────────────────────────────────────────┐
│                   Your PC (localhost)                 │
│                                                      │
│  ┌──────────┐  ┌──────────┐  ┌───────────────────┐  │
│  │ Mobile   │  │Backoffice│  │    Backend API    │  │
│  │ (Expo)   │  │ (Vite)   │  │  (ASP.NET Core)   │  │
│  │ :8080    │  │ :5173    │  │     :5000         │  │
│  └────┬─────┘  └────┬─────┘  └────────┬──────────┘  │
│       │              │                  │             │
│       └──────────────┴──────────────────┘             │
│                      │                                │
│              ┌───────┴───────┐                        │
│              │   PostgreSQL  │                        │
│              │    :5432      │                        │
│              ├───────────────┤                        │
│              │     Redis     │                        │
│              │    :6379      │                        │
│              └───────────────┘                        │
└──────────────────────────────────────────────────────┘
```

### How the Frontends Talk to the API

Both frontends make HTTP requests to `http://localhost:5000`:
- **Mobile**: Set via `EXPO_PUBLIC_API_URL` environment variable
- **Backoffice**: Set via `VITE_API_URL` environment variable

All API responses use a standard format:
```json
{
  "data": { ... }
}
```

Auth uses JWT tokens — the frontends store them securely and send them in the `Authorization: Bearer <token>` header.

## User Roles

| Role | Can Do |
|------|--------|
| **client** | Book appointments, view own history, manage profile |
| **therapist** | Manage schedule, availability, view clients, walk-ins |
| **admin** | Everything + dashboard, service management, user management |

## Troubleshooting

### "Port already in use"
```bash
# Find what's using the port
lsof -i :5000
# Kill it
kill -9 <PID>
```

### "Docker daemon not running"
Start Docker Desktop, then retry.

### API returns 401 Unauthorized
- Check you're sending the JWT token in the `Authorization` header
- Tokens expire after 60 minutes — use `/api/auth/refresh` to get a new one

### Mobile app can't connect to API
- Verify `EXPO_PUBLIC_API_URL=http://localhost:5000` is set
- Restart Expo after changing environment variables
- Check CORS: API allows `localhost:8080` and `localhost:8081`

### Backoffice shows blank page
- Verify `VITE_API_URL=http://localhost:5000` is set
- Restart Vite dev server after changing environment variables
- Check browser console for CORS errors

### Database issues / want a fresh start
```bash
docker compose down -v    # Deletes all data
docker compose up         # Recreates everything
```

### API won't build (manual setup)
```bash
cd ns-backend-api
dotnet restore
dotnet build
```

## API Endpoints Reference

### Authentication
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | No | Register new user |
| POST | `/api/auth/login` | No | Login, returns JWT |
| POST | `/api/auth/refresh` | No | Refresh access token |
| GET | `/api/auth/me` | Yes | Get current user |
| POST | `/api/auth/logout` | Yes | Logout |

### Appointments
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/appointments` | Yes | List appointments |
| GET | `/api/appointments/{id}` | Yes | Get by ID |
| POST | `/api/appointments` | Yes | Book appointment |
| PUT | `/api/appointments/{id}` | Yes | Update appointment |
| DELETE | `/api/appointments/{id}` | Therapist | Cancel appointment |
| POST | `/api/appointments/{id}/confirm` | Yes | Confirm appointment |
| POST | `/api/appointments/{id}/cancel` | Yes | Cancel appointment |
| GET | `/api/appointments/slots` | Yes | Get available slots |
| GET | `/api/appointments/upcoming` | Yes | Get upcoming |
| GET | `/api/appointments/client/{id}` | Yes | Client's history |
| GET | `/api/appointments/therapist/{id}` | Therapist | Therapist's schedule |

### Services
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/services` | No | List all service types |
| GET | `/api/services/active` | No | List active services |
| GET | `/api/services/{id}` | No | Get service by ID |
| POST | `/api/services` | Therapist | Create service type |
| PUT | `/api/services/{id}` | Therapist | Update service type |
| DELETE | `/api/services/{id}` | Therapist | Delete service type |

### Availability
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/availability/rules` | Yes | Get availability rules |
| POST | `/api/availability/rules` | Therapist | Create rule |
| PUT | `/api/availability/rules/{id}` | Therapist | Update rule |
| DELETE | `/api/availability/rules/{id}` | Therapist | Delete rule |
| GET | `/api/availability/blocks` | Yes | Get availability blocks |
| POST | `/api/availability/blocks` | Therapist | Create block |
| PUT | `/api/availability/blocks/{id}` | Therapist | Update block |
| DELETE | `/api/availability/blocks/{id}` | Therapist | Delete block |

### Dashboard
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/dashboard/stats` | Yes | Dashboard statistics |
| GET | `/api/dashboard/today` | Yes | Today's schedule |
| GET | `/api/dashboard/upcoming` | Yes | Upcoming appointments |
| GET | `/api/dashboard/at-risk` | Yes | At-risk appointments |

### Clients
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/clients` | Yes | List all clients |
| GET | `/api/clients/{id}` | Yes | Get client by ID |
| PUT | `/api/clients/{id}` | Yes | Update client |
| GET | `/api/clients/{id}/appointments` | Yes | Client's appointment history |

### Notifications
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/notifications` | Yes | Get user's notifications |
| GET | `/api/notifications/{id}` | Yes | Get notification by ID |
| PUT | `/api/notifications/{id}/read` | Yes | Mark as read |
| POST | `/api/notifications/mark-read` | Yes | Batch mark as read |

### Health Checks
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Basic health check |
| GET | `/health/ready` | Readiness check |

## Environment Variables

| Variable | Default | Used By |
|----------|---------|---------|
| `ConnectionStrings__DefaultConnection` | `Host=localhost;Port=5432;Database=massage_booking;Username=postgres;Password=postgres` | API |
| `ConnectionStrings__Redis` | `localhost:6379` | API |
| `Jwt__Key` | `YourSuperSecretKeyThatIsAtLeast32CharactersLong!` | API |
| `Jwt__Issuer` | `MassageBookingApi` | API |
| `Jwt__Audience` | `MassageBookingApp` | API |
| `Jwt__Expiry` | `60` (minutes) | API |
| `EXPO_PUBLIC_API_URL` | `http://localhost:5000` | Mobile |
| `VITE_API_URL` | `http://localhost:5000` | Backoffice |
