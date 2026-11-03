# IgloReservation API — Agent instructions

Project-level agent guidance for this repository. Cursor loads this file automatically as `AGENTS.md`.

The longer original brief lives at `agents/iglo-reservation-api.agent.md` (reference only; not auto-loaded). Prefer this file for session defaults; open the agents file when you need the full checklist.

## Mission

Build and maintain a production-ready ASP.NET Core Web API for SGK Üsküdar Iglo reservation management: layered architecture, PostgreSQL, EF Core Code First, AutoMapper, JWT auth, role-based authorization, validation, rate limiting, logging, and safe ProblemDetails errors.

## Locked stack and architecture

- **TFM:** .NET 10 (`net10.0`)
- **DB:** PostgreSQL + EF Core Code First migrations (Npgsql)
- **Mapping:** AutoMapper
- **Layers:** Controller → Service → Repository → DbContext (single API project, folder layers)
- **Auth:** ASP.NET Core Identity + JWT Bearer; roles `Admin` and `User`
- **Writes:** Admin only for reservations, iglo units, and user admin APIs; User is READ where authorized
- **Soft delete** on domain entities; store instants in **UTC**; business TZ **Europe/Istanbul**
- **Secrets:** User Secrets / environment only — never commit passwords or JWT signing keys
- **Migrations:** controlled deploy step; do not auto-`Migrate()` in production startup

## Working rules

1. Inspect existing code first; do not delete user changes without permission.
2. Present a short plan before non-trivial implementation; wait for approval when asked.
3. Keep business rules in services; no DbContext/repository use in controllers.
4. Do not return entities from endpoints; use DTOs.
5. Public register always assigns `User`; never Admin from the client.
6. Enforce reservation overlap in the service layer and preferably at the DB (exclusion/locking).
7. Run build/tests after meaningful changes; do not claim commands ran if they did not.
8. Do not drop/recreate the live database unless the user explicitly approves.

## Project AI assets (Cursor)

| Path | Role | Auto-loaded? |
|------|------|----------------|
| `AGENTS.md` (this file) | Project agent defaults | Yes |
| `.cursor/rules/*.mdc` | Always-on project rules | Yes (`alwaysApply: true`) |
| `.cursor/skills/*/SKILL.md` | Task skills (layered API, auth, EF/Postgres) | Yes (discovered by description) |
| `agents/iglo-reservation-api.agent.md` | Full original agent brief | No — attach with `@` or open when needed |
| `cursor/` | Legacy/source mirror of rules+skills | No — keep in sync manually if edited |

## Related skills

When relevant, follow:

- `.cursor/skills/aspnetcore-layered-api/SKILL.md`
- `.cursor/skills/authentication-authorization/SKILL.md`
- `.cursor/skills/postgresql-efcore-codefirst/SKILL.md`
