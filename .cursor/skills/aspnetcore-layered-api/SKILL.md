---
name: aspnetcore-layered-api
description: >-
  Builds and maintains the IgloReservation ASP.NET Core Web API using
  Controller → Service → Repository → DbContext, DTOs, AutoMapper, DI,
  validation, and ProblemDetails. Use when adding or changing controllers,
  services, repositories, DTOs, mapping profiles, middleware, or API endpoints.
---

# ASP.NET Core Layered API

## Layer responsibilities

- **Controller:** HTTP routing, model binding, authorization attributes, delegates to service, returns HTTP result. No DB access or business rules.
- **Service:** business rules, orchestration, validation that requires domain context, transaction boundary.
- **Repository:** persistence queries and writes; abstracts EF Core access where useful.
- **DTO:** API request/response contracts. Do not expose EF entities.
- **Entity:** persistence/domain data model.
- **DbContext:** EF Core unit of work and database mapping.
- **Mapping profile:** Entity ↔ DTO mapping through AutoMapper.

## Dependency direction

Controller → Service interface → Repository interface → DbContext.

Implementations are registered in dependency injection. Consumers depend on interfaces. Avoid circular dependencies and service locator patterns.

## Suggested structure

```text
src/IgloReservation.Api/
  Controllers/
  Services/
    Interfaces/
  Repositories/
    Interfaces/
  DTOs/
    Auth/
    Reservations/
    IgloUnits/
    Users/
  Models/
    Entities/
  Data/
    ApplicationDbContext.cs
    Configurations/
  Mapping/
    MappingProfile.cs
  Middleware/
  Program.cs
  appsettings.json
```

For a small project, keep one API project with clear folders. Do not create many projects solely to appear “clean architecture” unless the codebase benefits.

## Implementation steps

1. Define contracts and entity responsibilities.
2. Add DTO validation attributes or FluentValidation if selected.
3. Implement repository methods with async EF Core calls.
4. Implement services and business rules.
5. Implement controllers with route attributes and authorization.
6. Register interfaces and implementations in DI.
7. Add integration/unit tests where feasible.

## Async and query practices

- Use `async`/`await` and EF Core async methods for I/O.
- Pass `CancellationToken` through controller, service, and repository.
- Use `AsNoTracking()` for read-only queries.
- Apply pagination to collection endpoints.
- Project or map only required fields; avoid loading unbounded graphs.
- Do not return `IQueryable` from repository/service to controllers.

## AutoMapper

- Install the supported AutoMapper NuGet package(s) compatible with the chosen .NET version.
- Create a `MappingProfile : Profile` class and define mappings explicitly.
- Register AutoMapper in DI using the API appropriate to the installed package version.
- Use mapping for DTO/entity conversions, but do not rely on mapping to enforce business rules or authorization.
- Do not map sensitive fields such as password hashes, security stamps, or internal audit metadata to public DTOs.
- Verify package documentation/version before generating installation or registration code; do not invent package APIs.

## Transactions and consistency

- Multi-step writes that must succeed/fail together should use a transaction or a clear unit-of-work strategy.
- Reservation overlap checks must run server-side. Consider concurrency: two simultaneous requests may pass an application-level check. Use appropriate PostgreSQL constraints/locking/transaction strategy where feasible and translate conflict errors to `409 Conflict`.
