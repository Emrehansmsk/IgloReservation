---
name: authentication-authorization
description: >-
  Implements ASP.NET Core Identity + JWT Bearer auth and Admin/User role
  authorization for IgloReservation. Use when working on login, register,
  tokens, roles, Admin bootstrap, [Authorize] attributes, ownership checks,
  or auth-related tests and rate limits.
---

# Authentication and Authorization

## Preferred design

Use ASP.NET Core Identity with PostgreSQL for users and password hashing. Issue JWT Bearer tokens for API authentication. Use roles exactly:
- `Admin`
- `User`

## Rules

- Public registration, if enabled, always assigns `User`; ignore/reject any client-supplied role.
- Admin provisioning is private and secure (environment secrets, one-time command, or controlled operator action).
- Do not return password, password hash, security stamp, or secrets in DTOs.
- JWT signing key must be strong, secret, and supplied through user secrets/environment/secret store.
- Validate issuer, audience, signing key and token lifetime where configured.
- Keep token lifetime reasonable. Consider refresh tokens only if required; if implemented, store/rotate/revoke securely.
- Configure authentication before authorization middleware.
- Apply `[Authorize]` to protected endpoints and `[Authorize(Roles = "Admin")]` to writes/admin endpoints.
- `User` can access only explicitly permitted read endpoints. No write endpoint should be accessible to `User`.
- A 401 means unauthenticated/invalid token; 403 means authenticated but unauthorized.
- Never trust role or user ID supplied in request body for access control.
- For user-specific data, enforce ownership in service/query, not merely by hiding buttons in frontend.
- Avoid returning detailed login failure reasons that enable account enumeration.
- Apply rate limits to login and other sensitive endpoints.
- Add tests for unauthenticated, User, and Admin scenarios.

## Token contents

Include only necessary claims (subject/user id, role, and minimal identity claims). Do not include sensitive personal data. Avoid stale role claims for long-lived tokens; choose an appropriate expiry and revocation strategy.

## Authorization matrix

| Operation | Anonymous | User | Admin |
|---|---:|---:|---:|
| Login | Allow | Allow | Allow |
| Read reservation list/detail | Deny | Allow | Allow |
| Create reservation | Deny | Deny | Allow |
| Update reservation | Deny | Deny | Allow |
| Delete reservation | Deny | Deny | Allow |
| Read iglo units | Deny | Allow | Allow |
| Create/update/delete iglo units | Deny | Deny | Allow |
| Admin user management | Deny | Deny | Allow |
