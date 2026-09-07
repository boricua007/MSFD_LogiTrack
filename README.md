# MSFD LogiTrack

## Overview

LogiTrack is a warehouse and inventory management API built with ASP.NET Core and Entity Framework Core. It tracks inventory items, orders, and the relationship between them, using a code-first EF Core model backed by a SQLite database.

The application demonstrates a full data-access pipeline: defining domain models, configuring a `DbContext`, generating and applying migrations, and seeding/querying related data through EF Core.

## Features

✅ ASP.NET Core Web API with OpenAPI (Swagger) support  
✅ Entity Framework Core code-first modeling  
✅ SQLite database with migration history  
✅ One-to-many relationship between `Order` and `InventoryItem`  
✅ Seed data and console verification of persisted records  
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

## Project Structure

```
MSFD_LogiTrack/
│
├── Models/
│   ├── InventoryItem.cs
│   ├── Order.cs
│   └── LogiTrackContext.cs
├── Migrations/
├── Program.cs
├── appsettings.json
├── README.md
└── MSFD_LogiTrack.csproj
```

## How It Works

1. `LogiTrackContext` configures EF Core to use SQLite as the data provider.
2. `InventoryItem` and `Order` are defined as related entities, with each order holding a collection of inventory items.
3. On startup, the app ensures the database exists and seeds sample inventory items and an order if none are present.
4. Data is retrieved using EF Core queries (including `Include` for eager loading of related items) and printed to the console for verification.

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

## About

.NET 10 Web API built for the Microsoft Full Stack Developer course as part of the Full-Stack Certification track, Deployment and DevOps capstone (Part 1). This project establishes the core domain models and Entity Framework Core persistence layer for a warehouse inventory tracking system, reinforcing concepts including code-first modeling, database migrations, entity relationships, and data seeding/querying.

## Author

Daisy Viruet-Allen (boricua007)

GitHub: [https://github.com/boricua007](https://github.com/boricua007)
