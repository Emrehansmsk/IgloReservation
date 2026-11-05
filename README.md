# IgloReservation

ASP.NET Core Web API + React frontend for SGK Üsküdar Iglo reservation management.

- **API TFM:** `net10.0` (.NET 10 LTS)
- **Stack:** PostgreSQL, EF Core Code First, ASP.NET Core Identity, JWT Bearer, AutoMapper
- **Frontend:** Vite + React + TypeScript + Tailwind (`web/`)
- **Architecture (API):** Controller → Service → Repository → DbContext

## Status

- Identity + JWT auth; reservations / iglo units / admin users
- Soft delete, ProblemDetails, rate limiting, overlap exclusion constraint
- Reservation free-text `Description` (board cell content)
- Migrations: `InitialCreate`, `AddReservationDescription`
- React board: login/register, daily grid, date navigation, Admin CRUD modal
- Dev CORS allows `http://localhost:5173`

## Prerequisites

- .NET 10 SDK
- Node.js 20+ (frontend)
- PostgreSQL 18+
- `dotnet-ef` 10.x
- Secrets via User Secrets / environment — never commit passwords or JWT keys

## Configure secrets (API)

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\set-postgres-user-secrets.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\set-admin-bootstrap-user-secrets.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\verify-user-secrets.ps1
```

## Run locally

Terminal 1 — API:

```powershell
cd src\IgloReservation.Api
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --launch-profile http
```

API: http://localhost:5066 (Swagger: `/swagger`)

Terminal 2 — Frontend:

```powershell
cd web
copy .env.example .env   # if needed; default API URL is http://localhost:5066
npm install
npm run dev
```

UI: http://localhost:5173

1. Login with Admin (bootstrap) or register a User.
2. Ensure iglo units exist (board Admin panel or Swagger `POST /api/iglo-units`).
3. Admin: click a grid cell to create/edit/delete; put Excel-style text in **Açıklama** (`Description`).
4. Use date controls to move between days (slots are Europe/Istanbul: 09–12, 13–16, 16–19, 19–22).

## Swagger (Development only)

| URL | Purpose |
|-----|---------|
| http://localhost:5066/swagger | Swagger UI |
| `/swagger/v1/swagger.json` | OpenAPI document |

Authorize with JWT from `/api/auth/login` (paste token without `Bearer ` prefix).

## Build and test

```bash
dotnet restore IgloReservation.sln
dotnet build IgloReservation.sln -c Release
dotnet test IgloReservation.sln -c Release

cd web
npm run build
```

## EF migrations

```bash
cd src/IgloReservation.Api
set ASPNETCORE_ENVIRONMENT=Development
dotnet ef database update
```

Do **not** auto-run `Database.Migrate()` on production startup.

## Locked product decisions (summary)

See `docs/api-backend-plan.md`:

- Public register → `User` only
- Admin and User both see all reservations (board); writes remain Admin only
- Status: Pending, Confirmed, Cancelled, Completed
- Soft delete; Europe/Istanbul business TZ; UTC storage
- Board details live in `Description` text (no separate guest/deposit fields in v1)
- Overlap: service + DB exclusion

## Docker (API + PostgreSQL)

Frontend stays on Vercel (or similar). Backend stack:

```powershell
copy .env.example .env
# Edit .env: POSTGRES_PASSWORD, JWT_SIGNING_KEY, CORS_ORIGIN_0, AdminBootstrap, …

docker compose up -d --build
docker compose --profile migrate run --rm migrate
```

- API: `http://localhost:8080` (or `API_PUBLISH_PORT`)
- Health: `GET /health` (anonymous; checks Postgres via EF)
- Postgres: Docker network only (not published)
- TLS reverse proxy: set `API_HOST`, DNS A record, then `docker compose --profile tls up -d`

Do **not** commit `.env`. Production: no auto-`Migrate()` on API startup — use the `migrate` profile (or run `dotnet ef` against the DB).

## Solution layout

```text
IgloReservation/
  IgloReservation.sln
  AGENTS.md
  docker-compose.yml
  .env.example
  deploy/Caddyfile
  docs/api-backend-plan.md
  scripts/
  src/IgloReservation.Api/
  tests/IgloReservation.Api.Tests/
  web/                    # React + Vite + Tailwind
  README.md
  .cursor/
  agents/
```
