# IgloReservation API — backend plan (as implemented)

Living summary of locked decisions and what is in the codebase / local database. Update this file when product decisions change.

## Stack

| Item | Choice |
|------|--------|
| Runtime | .NET 10 (`net10.0`) |
| API | ASP.NET Core Web API |
| Database | PostgreSQL |
| ORM | EF Core Code First + migrations |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Mapping | AutoMapper |
| Architecture | Controller → Service → Repository → DbContext |

## Roles and auth

- Roles: `Admin`, `User`
- Public register always assigns `User`; never Admin from the client
- Admin provisioning: `AdminBootstrap` via User Secrets / env; idempotent (existing user → no password/role changes)
- No refresh tokens in v1
- Rate limits: `auth` (login/register), `api` (authenticated routes)

## Authorization matrix (implemented)

| Operation | Anonymous | User | Admin |
|-----------|-----------|------|-------|
| Login / register | Allow | Allow | Allow |
| `GET /api/auth/me` | Deny | Allow | Allow |
| List/detail reservations | Deny | All | All |
| Create/update/delete reservations | Deny | Deny | Allow |
| List/detail iglo units | Deny | Allow | Allow |
| Create/update/delete iglo units | Deny | Deny | Allow |
| User admin (`/api/users`) | Deny | Deny | Allow |

> Board visibility: Admin and User both read all reservations. Create/update/delete remain Admin-only.

## Domain

- `ApplicationUser` (Identity)
- `IgloUnit` — soft delete + `IsActive`
- `Reservation` — UTC range, status enum, soft delete
- `AuditLog` — admin soft-delete actions (expand later)
- Statuses: `Pending`, `Confirmed`, `Cancelled`, `Completed`
- Conflict-active statuses: `Pending` + `Confirmed`
- Soft delete via `IsDeleted` / `DeletedAtUtc` + EF global query filters
- Business timezone: `Europe/Istanbul`; persist instants as UTC (`timestamp with time zone`)
- `Description` (nullable string, max 1000): free-form board cell text (guest/deposit/notes combined)

## Overlap enforcement

1. **Service:** `ReservationService.EnsureNoActiveOverlapAsync` before create/update
2. **Database:** partial GiST exclusion constraint `ex_reservations_active_overlap` on `reservations`  
   - Requires extension `btree_gist`  
   - Predicate: `Status IN ('Pending','Confirmed') AND IsDeleted = FALSE`  
   - Range: `tstzrange(StartAtUtc, EndAtUtc, '[)')`
3. Exclusion violations map to HTTP **409** in `ExceptionHandlingMiddleware`

## Schema / migrations

- Migration: `20261102193621_InitialCreate`
- Applied locally to database `IgloReservationDb`
- Tables: Identity (`AspNet*`), `iglo_units`, `reservations`, `audit_logs`, `__EFMigrationsHistory`

## Endpoints (current)

- `api/auth` — login, register, me
- `api/reservations` — CRUD (writes Admin-only)
- `api/iglo-units` — CRUD (writes Admin-only)
- `api/users` — Admin list/get/set-role

## Frontend (v1)

- App under `web/` (Vite + React + TypeScript + Tailwind)
- Dev CORS: `http://localhost:5173`
- Daily grid consumes units + reservations; Admin modal writes `Description` + status + slot times

## Open / next items

- Health check endpoint
- Optional: expand audit coverage; structured guest/deposit fields if product outgrows `Description`
- Iglo unit management UI (currently API/Swagger)

## AdminBootstrap (implemented)

- Options: `AdminBootstrap:Email`, `AdminBootstrap:Password` (User Secrets / env only)
- Hosted service ensures roles; creates Admin user when email is unused
- Existing email: leave password and roles unchanged
- Partial/invalid config → `AdminBootstrapException` (fails startup; no secret logging)
