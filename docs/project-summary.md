# LogiTrack — Project Summary (Peer Review)

## Overview

LogiTrack is an ASP.NET Core Web API for warehouse inventory and order management, backed by EF Core + SQLite, secured with ASP.NET Core Identity + JWT bearer auth, and optimized with in-memory caching.

## Core Components

- **Persistence:** `LogiTrackContext` (EF Core, SQLite) with code-first migrations; `InventoryItem` and `Order` in a one-to-many relationship.
- **Auth:** `ApplicationUser` extends `IdentityUser`; `AuthController` issues signed JWTs on login with role claims (`Manager`/`User`). Password policy hardened (min length 8, non-alphanumeric + uppercase required) with account lockout after 5 failed attempts.
- **Authorization:** All `Inventory`/`Order` endpoints require a valid JWT; inventory deletion additionally requires the `Manager` role.
- **Caching:** `IMemoryCache` fronts the two list-read endpoints (`GET /api/inventory`, `GET /api/order`) with a 30s absolute expiration, invalidated on every mutation — including cross-invalidating each other's cache key, since an `InventoryItem` can appear both standalone and nested inside an `Order`.
- **DTOs:** `ItemDto`/`OrderDto` decouple API contracts from EF entities, with data-annotation validation (`[Required]`, `[Range]`) enforced via `ModelState`.
- **Error handling:** `AddProblemDetails()` + `UseExceptionHandler()` return a consistent RFC 7807 JSON shape for unhandled exceptions instead of raw 500s/stack traces.

## Key Decisions

1. **State persistence strategy:** chose the **database strategy** (SQLite via EF Core) as the system of record, since `IMemoryCache` is non-durable and doesn't survive restarts or scale across instances. Cache is used only as a performance layer with write-invalidation.
2. **Fixed a Swagger "Failed to fetch" bug** caused by `UseHttpsRedirection()` redirecting `http://localhost:5085` requests to a different HTTPS origin/port, which the browser's `fetch()` couldn't follow cross-origin. Redirection is now skipped in Development.
3. **Redundant-logic cleanup:** distinguished previously-duplicated seed data literals across demo/test blocks so each is traceable to one place; cross-invalidated caches between controllers to prevent stale-data drift for shared item rows.
4. **Global exception handling** added for consistent, non-leaky error responses in production.

## Known Trade-offs

Documented rather than fixed, given the scope of a capstone demo:

- CORS is currently permissive (`AllowAnyOrigin`) to simplify Swagger UI testing.
- JWT signing key is a placeholder value in `appsettings.json` rather than pulled from a secret store.
- No structured file logging, response pagination, or automated test suite yet.

See [architecture.md](architecture.md) for the request-flow diagram and detailed data-flow description.
