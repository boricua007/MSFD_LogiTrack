# MSFD LogiTrack

## Overview

LogiTrack is a warehouse and inventory management API built with ASP.NET Core, Entity Framework Core, and SQLite. It exposes REST endpoints for inventory items and orders, documents those endpoints with Swagger/OpenAPI, and uses DTOs to keep API responses separate from the EF Core entities.

The application demonstrates a full data-access pipeline: defining domain models, configuring a `DbContext`, generating and applying migrations, seeding/querying related data through EF Core, and serving asynchronous controller actions with validation and error handling.

## Features

✅ ASP.NET Core Web API with OpenAPI (Swagger) support and interactive endpoint testing  
✅ Entity Framework Core code-first modeling  
✅ SQLite database with migration history  
✅ One-to-many relationship between `Order` and `InventoryItem`  
✅ Seed data and console verification of persisted records  
✅ Inventory and order CRUD endpoints for the implemented operations  
✅ `ItemDto` and `OrderDto` request/response contracts  
✅ Async database access, validation, and meaningful HTTP error responses  
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
    https://localhost:<port>/swagger
    ```

    The port is printed by `dotnet run` and is also configured in `Properties/launchSettings.json`.

## API Endpoints

The controllers use the route prefix `api/[controller]`.

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

## Project Structure

```
MSFD_LogiTrack/
│
├── Models/
│   ├── InventoryItem.cs
│   ├── Order.cs
│   └── LogiTrackContext.cs
├── Controllers/
│   ├── InventoryController.cs
│   └── OrderController.cs
├── DTOs/
│   ├── ItemDto.cs
│   └── OrderDto.cs
├── Migrations/
├── Program.cs
├── appsettings.json
├── README.md
└── MSFD_LogiTrack.csproj
```

## How It Works

1. `Program.cs` registers controllers, JSON cycle handling, Swagger services, and `LogiTrackContext` with SQLite.
2. `InventoryItem` and `Order` are defined as related entities, with each order holding a collection of inventory items.
3. On startup, the app ensures the database exists and seeds sample inventory items and an order if none are present.
4. Controllers use asynchronous EF Core queries and `Include` to load related order items.
5. Controller actions map entities to `ItemDto` and `OrderDto` objects before returning API responses.
6. The development environment enables Swagger UI for trying the endpoints in a browser.
7. The startup test blocks also print sample entity and order output to the console for verification.

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

## About

LogiTrack is the final capstone project for the Microsoft Full Stack Developer course and the Full-Stack Certification track. It is a .NET 10 warehouse and inventory management API designed to model common logistics workflows: maintaining inventory, creating customer orders, and associating order items with persisted records.

The project brings together ASP.NET Core Web API, Entity Framework Core, SQLite, database migrations, DTO-based request and response contracts, and Swagger/OpenAPI documentation. It demonstrates the progression from domain models and relational persistence to asynchronous controller endpoints with validation, error handling, JSON serialization, and interactive API testing.

## Author

Daisy Viruet-Allen (boricua007)

GitHub: [https://github.com/boricua007](https://github.com/boricua007)
