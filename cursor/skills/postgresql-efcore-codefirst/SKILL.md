---
name: postgresql-efcore-codefirst
description: Create PostgreSQL database schema from C# entities using EF Core Code First migrations, configure connection strings, and manage schema changes safely.
---

# PostgreSQL + EF Core Code First

## Required approach

- PostgreSQL is the relational database.
- EF Core is the ORM.
- Entity classes and EF Core configuration define schema.
- Migrations create/update schema; do not manually create production tables outside migrations.
- DbContext and configurations belong under `Data`.

## Packages

Select compatible versions for the chosen .NET target and install:
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` if Identity is used
- `AutoMapper` package(s) required by the selected version

Use `dotnet add package <package>` and verify the installed package API. Do not pin arbitrary versions without checking compatibility.

## Connection configuration

Use a named connection string in `appsettings.json`, for example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=IgloReservationDb;Username=app_user;Password=REPLACE_ME"
  }
}
```

This is a placeholder only. Do not commit real passwords. Use .NET User Secrets locally and environment variables/secrets in deployment.

## DbContext

- Use `ApplicationDbContext` under `Data`.
- If ASP.NET Core Identity is selected, inherit from the appropriate Identity DbContext type.
- Configure entity relationships, indexes, precision, max lengths, required fields, and delete behavior.
- Use explicit entity configurations under `Data/Configurations` for nontrivial mappings.
- PostgreSQL table and column naming should be consistent; avoid accidental mixed-case quoted identifiers.

## Migration workflow

From the project directory:

```bash
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If tool already exists, update/verify rather than reinstalling blindly.

For separate startup/project paths, specify `--project` and `--startup-project`.

For deployed environments:
- generate migration locally and review it;
- deploy application and migration artifact;
- apply migration as a controlled deployment step;
- back up DB before production migration;
- do not automatically run `Database.Migrate()` on every production app startup unless explicitly designed and reviewed.

## Data seeding

- Seed only deterministic, non-secret reference data via EF Core seeding or a controlled initializer.
- Bootstrap the first admin through a secure one-time command/initializer using environment-provided credentials.
- Never hardcode a production admin password or expose admin creation through public registration.
- Ensure migration and seed approach is idempotent.

## PostgreSQL-specific

- Use Npgsql provider.
- Consider PostgreSQL range/exclusion constraints for reservation overlap prevention if the domain requires it.
- Ensure date/time semantics are explicit; prefer UTC storage (`timestamp with time zone`) for instants and document local business timezone.
- Add indexes for common filters such as reservation date range, unit, user, and status.
- Configure sensible connection pooling and command timeout; avoid premature tuning.
