# APIDoctorCheckUp — API

A real-time REST API health monitoring backend built with .NET 10 Clean Architecture. Monitors configured endpoints on independent background schedules, evaluates alert thresholds, tracks incidents, and streams live results to connected clients via SignalR.

**Live API:** https://apidoctorcheckup-api.onrender.com  
**Live Dashboard:** https://api-doctor-check-up-client.vercel.app  
**API Docs (Scalar):** available at `/scalar/v1` in development mode

---

## Tech Stack

| Concern | Technology |
|---|---|
| Framework | .NET 10 Web API — Controllers pattern |
| Real-time | SignalR |
| Background processing | IHostedService / BackgroundService |
| ORM | EF Core 10 |
| Local database | SQLite |
| Production database | PostgreSQL on Neon.tech |
| Auth | JWT Bearer |
| API docs | Scalar |
| Containerisation | Docker multi-stage build |
| Deployment | Render.com |
| Testing | xUnit + Moq |

---

## Architecture

Clean Architecture with strict dependency rules:

```
Domain          ← zero dependencies
    ↑
Application     ← depends on Domain only
    ↑
Infrastructure  ← depends on Application
    ↑
Api             ← composition root only
```

Controllers inject Application interfaces only. Infrastructure types never leak into the Api layer. All EF Core mapping lives in `IEntityTypeConfiguration` classes — domain entities have no framework annotations.

---

## Features

- Configurable endpoint monitoring — URL, name, expected status code, check interval
- Per-endpoint independent background workers with staggered startup
- Per-check recording — timestamp, response time, status code, success/failure, error message
- Alert threshold evaluation — response time warning/critical, consecutive failures
- Incident open/close lifecycle — automatically opened on failure, closed on recovery
- Uptime calculation — 24h, 7d, 30d windows
- Real-time SignalR push on every check result and status transition
- Full REST API with JWT-protected write endpoints
- Single admin account via environment variables — no user registration
- Docker multi-stage build with non-root runtime user
- Migrate-on-start — database created automatically on first run

---

## REST API

| Method | Route | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/login` | Public | Get JWT token |
| GET | `/api/dashboard/summary` | Public | All endpoints with current status |
| GET | `/api/endpoints` | Public | List all endpoints |
| GET | `/api/endpoints/{id}` | Public | Single endpoint |
| POST | `/api/endpoints` | JWT | Create endpoint |
| PUT | `/api/endpoints/{id}` | JWT | Update endpoint |
| DELETE | `/api/endpoints/{id}` | JWT | Delete endpoint |
| GET | `/api/endpoints/{id}/checks` | Public | Recent check results |
| GET | `/api/endpoints/{id}/stats` | Public | Uptime and incident stats |
| GET | `/health` | Public | Health check with DB probe |

### SignalR Hub — `/hubs/monitor`

| Event | Direction | Description |
|---|---|---|
| `JoinDashboard` | Client → Server | Subscribe to all updates |
| `OnCheckResult` | Server → Client | Latest check result |
| `OnStatusChanged` | Server → Client | Status transition |

---

## Local Setup

### Prerequisites

- .NET 10 SDK
- Docker Desktop (optional)

### Run with dotnet

```bash
git clone https://github.com/janlu89/APIDoctorCheckUp-API.git
cd APIDoctorCheckUp-API
dotnet run --project APIDoctorCheckUp.Api
```

The SQLite database is created automatically on first run. The API listens on `http://localhost:5292`.

### Run with Docker

```bash
docker build -f APIDoctorCheckUp.Api/Dockerfile -t apidoctorcheckup-api:local .
docker run --rm -p 8080:8080 -v apidoctor-data:/data apidoctorcheckup-api:local
```

The API listens on `http://localhost:8080`.

---

## Environment Variables

| Variable | Description | Default |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | SQLite (`Data Source=...`) or PostgreSQL connection string | SQLite local file |
| `JWT__Secret` | Signing key — minimum 32 characters | Required |
| `JWT__Issuer` | JWT issuer | `APIDoctorCheckUp` |
| `JWT__Audience` | JWT audience | `APIDoctorCheckUp` |
| `JWT__ExpiryHours` | Token lifetime in hours | `12` |
| `Admin__Username` | Admin login username | `admin` |
| `Admin__Password` | Admin login password | Required in production |
| `Cors__AllowedOrigins__0` | Allowed frontend origin | `http://localhost:4200` |

---

## Deploy Your Own

### 1 — Database (Neon.tech)

1. Create a free project at https://neon.tech
2. Copy the connection string (key-value format for Npgsql)

### 2 — Backend (Render.com)

1. Create a new Web Service at https://render.com
2. Connect your fork of this repository
3. Set Language to **Docker**, Dockerfile path to `./APIDoctorCheckUp.Api/Dockerfile`
4. Leave Root Directory empty
5. Add all environment variables from the table above
6. Deploy

The database schema and seed data are applied automatically on first start via `MigrateAsync`.

### 3 — Keep-alive (UptimeRobot)

Create a free monitor at https://uptimerobot.com pointing to `https://your-api.onrender.com/health` to prevent the Render.com free tier from sleeping after 15 minutes of inactivity.

---

## Running Tests

```bash
dotnet test
```

16 tests covering:

- `UptimeCalculator` — percentage calculation, time window filtering, edge cases (zero results, all failures, mixed)
- `AlertEvaluator` — status transitions, consecutive failure counting, incident open/close lifecycle, duplicate incident prevention
