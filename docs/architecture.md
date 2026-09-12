# LogiTrack Architecture

LogiTrack is an ASP.NET Core Web API backed by Entity Framework Core, ASP.NET Core Identity, and SQLite. The application secures routes with JWT authentication, caches read endpoints with `IMemoryCache`, separates HTTP contracts from persistence entities with DTOs, and exposes operations through controller endpoints.

```mermaid
flowchart TD
    %% Nodes
    Client(["🌐 API Client / Swagger UI"])
    AuthHeader["🔑 Authorization: Bearer JWT"]
    AuthCtrl["🔐 AuthController / JWT Middleware"]
    
    subgraph Controllers [API Controllers]
        Inventory["📦 InventoryController"]
        Orders["🛒 OrderController"]
    end

    subgraph Caching [Caching Layer]
        MemCache[("⚡ IMemoryCache")]
    end

    subgraph Contracts [Data Contracts]
        ItemDto["📄 ItemDto"]
        OrderDto["📋 OrderDto"]
    end

    subgraph Persistence [Data Access Layer]
        Context["⚙️ LogiTrackContext"]
        UserManager["👤 Identity UserManager"]
        SQLite[("🗄️ SQLite Database")]
        Migrations["🛠️ EF Core Migrations"]
        Order["📋 Order Entity"]
        Items["📦 InventoryItem Entity"]
        AppUser["👤 ApplicationUser Entity"]
    end

    %% Flow Connections
    Client --> AuthHeader
    AuthHeader --> AuthCtrl
    AuthCtrl -->|Authenticate / Authorize| Controllers

    Inventory <-->|Cache Hit / Miss| MemCache
    Orders <-->|Cache Hit / Miss| MemCache

    Inventory --> ItemDto
    Orders --> OrderDto
    OrderDto --> ItemDto

    Inventory --> Context
    Orders --> Context
    AuthCtrl --> UserManager
    UserManager --> Context

    Context --> SQLite
    Migrations --> SQLite

    Order --> Items
    AppUser --> Context

    %% Styling Definitions
    classDef client fill:#24292f,stroke:#58a6ff,stroke-width:2px,color:#ffffff
    classDef security fill:#D83B01,stroke:#ff9f70,stroke-width:2px,color:#ffffff
    classDef compute fill:#0078D4,stroke:#50e6ff,stroke-width:2px,color:#ffffff
    classDef cache fill:#107C10,stroke:#54d454,stroke-width:2px,color:#ffffff
    classDef dto fill:#008272,stroke:#00e5ce,stroke-width:2px,color:#ffffff
    classDef database fill:#512BD4,stroke:#a78bfa,stroke-width:2px,color:#ffffff

    %% Class Assignments
    class Client,AuthHeader client
    class AuthCtrl,UserManager,AppUser security
    class Inventory,Orders compute
    class MemCache cache
    class ItemDto,OrderDto dto
    class Context,SQLite,Migrations,Order,Items database
```

## Request Flow

1. A client or Swagger UI sends an HTTP request with a `Bearer <JWT>` token in the `Authorization` header.
2. The JWT Middleware authenticates the request and validates claims/roles (`[Authorize]`).
3. For GET endpoints (`/api/inventory`, `/api/order`), the controller checks `IMemoryCache`:
   - **Cache Hit:** Returns cached DTO response immediately without querying the database.
   - **Cache Miss:** Queries `LogiTrackContext` using `.AsNoTracking()` and direct LINQ `.Select()` projections, stores the result in `IMemoryCache` with a 30-second sliding/absolute policy, and returns the response.
4. Mutation endpoints (`POST`, `DELETE`) write changes to `LogiTrackContext`, persist to SQLite, and invalidate the corresponding cache entries.
5. Swagger/OpenAPI documents available routes and provides Bearer token authorization support.

## Persistence & Security Model

- **Identity & Auth:** `ApplicationUser` extends `IdentityUser`. `AuthController` handles registration and login, issuing signed JWT tokens.
- **Entities & Relationships:** `Order` represents a customer order and contains a collection of `InventoryItem` records. `InventoryItem` stores item details and an optional `OrderId` foreign key.
- **Caching & Query Optimization:** Direct LINQ projections prevent over-fetching entity columns, and `IMemoryCache` minimizes database roundtrips for repeated read requests.
- **Migrations & Seeding:** EF Core migrations track database schema changes. Startup logic seeds default inventory, orders, and identity roles when empty.
