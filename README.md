# MSFD LogiTrack

## Overview

LogiTrack is a warehouse and inventory management API built with ASP.NET Core, Entity Framework Core, and SQLite. It exposes secure REST endpoints for inventory items and orders, documents those endpoints with Swagger/OpenAPI, and uses DTOs to keep API responses separate from EF Core entities.

The application demonstrates a full data-access and security pipeline: defining domain models, configuring a `DbContext`, generating EF Core migrations, securing endpoints with ASP.NET Core Identity and JWT bearer tokens, and optimizing endpoint speed using `IMemoryCache` and direct LINQ query projections.

## Features

✅ ASP.NET Core Web API with OpenAPI (Swagger) support and interactive endpoint testing  
✅ Entity Framework Core code-first modeling  
✅ SQLite database with migration history  
✅ One-to-many relationship between `Order` and `InventoryItem`  
✅ Seed data and console verification of persisted records  
✅ Inventory and order CRUD endpoints for the implemented operations  
✅ `ItemDto` and `OrderDto` request/response contracts  
✅ Async database access, validation, and meaningful HTTP error responses  
✅ ASP.NET Core Identity user accounts backed by EF Core/SQLite  
✅ JWT bearer authentication with registration and login endpoints  
✅ Role-based authorization (`[Authorize]` / `[Authorize(Roles = "Manager")]`)  
✅ In-memory caching with `IMemoryCache` and automatic cache invalidation on mutation  
✅ Optimized EF Core queries with `.AsNoTracking()` and direct LINQ projections  
✅ Account lockout and password policy hardening to mitigate brute-force attacks  
✅ Clean, well-structured project layout

## Getting Started

1. Clone the repository

   ```powershell
   git clone https://github.com/boricua007/MSFD_LogiTrack.git
   cd MSFD_LogiTrack
   ```

2. Install the EF Core CLI tools (if not already installed)

   ```powershell
   dotnet tool install --global dotnet-ef
   ```

3. Apply migrations to create the database

   ```powershell
   dotnet ef database update
   ```

4. Run the application

   ```powershell
   dotnet run
   ```

5. Open Swagger UI while the application is running

    ```text
    http://localhost:<port>/swagger
    ```

    The port is printed by `dotnet run` and is also configured in `Properties/launchSettings.json`. Swagger UI is only enabled in the Development environment, so set `ASPNETCORE_ENVIRONMENT=Development` first if it isn't already set:

    ```powershell
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    dotnet run
    ```

6. Register a user, log in, and authorize Swagger

    - Call `POST /api/auth/register` with an email/password to create an account.
    - Call `POST /api/auth/login` with the same credentials to receive a JWT in the response body.
    - Click the **Authorize** button in Swagger UI and paste just the token value (no `Bearer` prefix, no surrounding braces/quotes).
    - Protected endpoints (e.g. `GET /api/inventory`) now return `200 OK`; without a token they return `401 Unauthorized`.

## API Endpoints

The controllers use the route prefix `api/[controller]`.

### Auth

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/api/auth/register` | Create a new Identity user account. |
| `POST` | `/api/auth/login` | Validate credentials and return a signed JWT. |

Example `POST /api/auth/register` / `login` request:

```json
{
   "email": "jmorris@example.com",
   "password": "StrongPass1!"
}
```

All `Inventory` and `Order` endpoints require `Authorization: Bearer <token>`. The inventory delete endpoint additionally requires the `Manager` role.

### Inventory

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/inventory` | Return all inventory items as `ItemDto` objects. |
| `POST` | `/api/inventory` | Create an inventory item and return the generated ID. |
| `DELETE` | `/api/inventory/{id}` | Delete an inventory item by ID. |

Example `POST /api/inventory` request:

```json
{
   "name": "Pallet Jack",
   "quantity": 12,
   "location": "Warehouse A"
}
```

### Orders

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/order` | Return all orders with their related items. |
| `GET` | `/api/order/{id}` | Return one order with its related items. |
| `POST` | `/api/order` | Create an order and its item records. |
| `DELETE` | `/api/order/{id}` | Delete an order by ID. |

Example `POST /api/order` request:

```json
{
   "customerName": "Samir",
   "items": [
      {
         "name": "Pallet Jack",
         "quantity": 12,
         "location": "Warehouse A"
      },
      {
         "name": "Forklift",
         "quantity": 3,
         "location": "Warehouse B"
      }
   ]
}
```

An order must include at least one item. Missing inventory or order IDs return `404 Not Found`; an order without items returns `400 Bad Request`; successful deletes return `204 No Content`.

## DTOs

The API uses data transfer objects instead of exposing EF Core entities directly:

- `ItemDto` contains `ItemId`, `Name`, `Quantity`, and `Location`.
- `OrderDto` contains `OrderId`, `CustomerName`, and an `Items` collection of `ItemDto` objects.
- `OrderDto.Items` is also used to submit the item data when creating an order.

## Architecture

See the [LogiTrack architecture diagram](docs/architecture.md) for an overview of the request flow, DTO mapping, EF Core persistence, SQLite database, and order-item relationship.

## Project Structure

```
MSFD_LogiTrack/
│
├── Models/
│   ├── InventoryItem.cs
│   ├── Order.cs
│   ├── ApplicationUser.cs
│   └── LogiTrackContext.cs
├── Controllers/
│   ├── InventoryController.cs
│   ├── OrderController.cs
│   └── AuthController.cs
├── DTOs/
│   ├── ItemDto.cs
│   ├── OrderDto.cs
│   ├── RegisterDto.cs
│   └── LoginDto.cs
├── docs/
│   └── architecture.md
├── Migrations/
├── Program.cs
├── appsettings.json
├── README.md
└── MSFD_LogiTrack.csproj
```

## How It Works

1. `Program.cs` registers controllers, JSON cycle handling, Swagger services (with a JWT Authorize button), `LogiTrackContext` with SQLite, ASP.NET Core Identity, and JWT bearer authentication.
2. `InventoryItem` and `Order` are defined as related entities, with each order holding a collection of inventory items. `ApplicationUser` extends `IdentityUser` for account data.
3. On startup, the app ensures the database exists and seeds sample inventory items and an order if none are present.
4. `AuthController` registers users via `UserManager`, validates credentials via `SignInManager`, and issues a signed JWT containing the user's ID, email, and role claims.
5. Controllers use asynchronous EF Core queries and `Include` to load related order items, and are secured with `[Authorize]`; the inventory delete endpoint additionally requires the `Manager` role.
6. Controller actions map entities to `ItemDto` and `OrderDto` objects before returning API responses.
7. The development environment enables Swagger UI for trying the endpoints (including authenticating with a bearer token) in a browser.
8. The startup test blocks also print sample entity and order output to the console for verification.

## Sample Output

```
Order #1 for Samir | Items: 2 (Pallet Jack, Forklift) | Placed: 9/7/2026
Item: Pallet Jack | Quantity: 12 | Location: Warehouse A
Item: Forklift | Quantity: 3 | Location: Warehouse B
```

## Key Concepts Demonstrated

- Entity Framework Core code-first development
- One-to-many entity relationships
- Database migrations (`dotnet ef migrations add`, `dotnet ef database update`)
- SQLite as a lightweight relational database
- Dependency injection and `DbContext` scoping
- ASP.NET Core minimal hosting model
- Seeding and querying related data
- REST routing with controller actions
- DTO mapping and JSON serialization
- Swagger/OpenAPI endpoint documentation
- Async API operations and HTTP status codes
- ASP.NET Core Identity for user account management
- JWT bearer token issuance and validation
- Role-based authorization and account lockout policies

## About

LogiTrack is the final capstone project for the Microsoft Full Stack Developer course and the Full-Stack Certification track. It is a .NET 10 warehouse and inventory management API designed to model common logistics workflows: maintaining inventory, creating customer orders, and associating order items with persisted records.

The project brings together ASP.NET Core Web API, Entity Framework Core, SQLite, database migrations, DTO-based request and response contracts, Swagger/OpenAPI documentation, and ASP.NET Core Identity with JWT authentication and role-based authorization. It demonstrates the progression from domain models and relational persistence to asynchronous, secured controller endpoints with validation, error handling, JSON serialization, and interactive API testing.

## Author

Daisy Viruet-Allen (boricua007)

GitHub: [https://github.com/boricua007](https://github.com/boricua007)
