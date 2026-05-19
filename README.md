# StockPro — Backend API

> A microservices-based inventory management system built with **ASP.NET Core**, **Entity Framework Core**, **SQL Server**, and an **Ocelot API Gateway**.

---

## System Use Case Diagram

Based on the explicit Role-Based Access Control (RBAC) rules (ADMIN, MANAGER, OFFICER, STAFF), the following diagram maps out the primary actors and the operations they are authorized to perform.

```mermaid
flowchart LR
    %% Actors
    Admin((👤 Admin))
    Manager((👨‍💼 Manager))
    Officer((🏢 Officer))
    Staff((👷 Staff))
    System((⚙️ System Worker))

    %% System Boundary
    subgraph "StockPro System Boundary"
        direction TB
        
        %% Use Cases
        UC_Users([Manage Users & Access])
        
        UC_Products([Manage Products])
        UC_Reports([Generate Reports])
        UC_Movements([Review Stock Movements])
        UC_Alerts([Receive Low Stock Alerts])
        
        UC_Suppliers([Manage Suppliers])
        UC_PO([Create & Approve Purchase Orders])
        
        UC_Warehouses([Manage Warehouses])
        UC_Stock([Log Stock Movements])
        UC_Receive([Receive Goods from PO])
        
        UC_Monitor([Monitor Inventory Levels])
    end

    %% Admin Links
    Admin --> UC_Users
    Admin --> UC_Warehouses
    Admin --> UC_Movements

    %% Manager Links
    Manager --> UC_Products
    Manager --> UC_Reports
    Manager --> UC_Movements
    Manager -.->|Is Notified By| UC_Alerts

    %% Officer Links
    Officer --> UC_Suppliers
    Officer --> UC_PO
    
    %% Staff Links
    Staff --> UC_Stock
    Staff --> UC_Receive
    Staff -->|Views| UC_Warehouses

    %% System Background Links
    System --> UC_Monitor
    UC_Monitor -.->|Triggers| UC_Alerts

    %% Apply basic styling
    classDef actor fill:#e2e8f0,stroke:#64748b,stroke-width:2px,color:#0f172a
    classDef usecase fill:#dbeafe,stroke:#3b82f6,stroke-width:2px,color:#1e3a8a
    
    class Admin,Manager,Officer,Staff,System actor
    class UC_Users,UC_Products,UC_Reports,UC_Movements,UC_Alerts,UC_Suppliers,UC_PO,UC_Warehouses,UC_Stock,UC_Receive,UC_Monitor usecase
```

### Role Authorization Breakdown:
* **Admin:** Has system-wide access, including creating users and defining core infrastructure like warehouses.
* **Manager:** Primarily handles analytics, product catalogs, and receives alerts when stock dips below safe thresholds.
* **Officer:** Focused on procurement. They define suppliers and draft/approve Purchase Orders.
* **Staff:** The boots-on-the-ground warehouse workers. They physically receive goods and log ad-hoc stock movements.
* **System Worker:** The automated `AlertService` background job that monitors inventory levels seamlessly.

---

## Microservices Architecture & Connectivity

StockPro is structured as a collection of independent microservices, each owning its own database and exposed through a unified API Gateway.

```mermaid
graph TD
    %% Define Styles
    classDef client fill:#e0f2fe,stroke:#0284c7,stroke-width:2px,color:#0f172a;
    classDef gateway fill:#fef08a,stroke:#ca8a04,stroke-width:2px,color:#422006;
    classDef service fill:#dcfce7,stroke:#16a34a,stroke-width:2px,color:#14532d;
    classDef worker fill:#cffafe,stroke:#06b6d4,stroke-width:2px,color:#164e63;
    classDef database fill:#f3e8ff,stroke:#9333ea,stroke-width:2px,color:#3b0764;

    %% Client Layer
    Client["📱 Client Applications<br/>(React SPA)"]:::client
    
    %% API Gateway Layer
    Client -->|HTTP Requests| API_GW{"🛡️ API Gateway<br/>(Ocelot)"}:::gateway

    %% Microservices Layer
    subgraph "Microservices Cluster"
        direction TB
        
        AuthSvc["🔐 Auth Service"]:::service
        ProductSvc["📦 Product Service"]:::service
        WarehouseSvc["🏭 Warehouse & Stock Service"]:::service
        MovementSvc["🚚 Stock Movement Service"]:::service
        SupplierSvc["🤝 Supplier Service"]:::service
        POSvc["🛒 Purchase Order Service"]:::service
        ReportSvc["📊 Report Service"]:::service
        AlertSvc["⚠️ Alert Service<br/>(Background Worker)"]:::worker

        %% Gateway Routing
        API_GW -->|"/api/auth/*"| AuthSvc
        API_GW -->|"/api/products/*"| ProductSvc
        API_GW -->|"/api/warehouses/*<br/>/api/stock/*"| WarehouseSvc
        API_GW -->|"/api/movements/*"| MovementSvc
        API_GW -->|"/api/suppliers/*"| SupplierSvc
        API_GW -->|"/api/purchase-orders/*"| POSvc
        API_GW -->|"/api/reports/*"| ReportSvc
        API_GW -->|"/api/alerts/*"| AlertSvc

        %% Synchronous HTTP Inter-service communication
        WarehouseSvc -.->|"POST /api/movements"| MovementSvc
        POSvc -.->|"POST /api/stock/update"| WarehouseSvc
        ProductSvc -.->|"GET /api/stock/all"| WarehouseSvc
        ReportSvc -.->|"GET /api/stock/all"| WarehouseSvc
        ReportSvc -.->|"GET /api/products/[id]"| ProductSvc
        AlertSvc -.->|"GET /api/stock/all<br/>GET /api/warehouses/[id]"| WarehouseSvc
        AlertSvc -.->|"GET /api/products/[id]"| ProductSvc
    end

    %% Database Layer
    subgraph "Data Storage (SQL Server)"
        direction LR
        AuthDB[("🗄️ Auth DB")]:::database
        ProductDB[("🗄️ Product DB")]:::database
        WarehouseDB[("🗄️ Warehouse DB")]:::database
        MovementDB[("🗄️ Movement DB")]:::database
        SupplierDB[("🗄️ Supplier DB")]:::database
        PODB[("🗄️ Purchase DB")]:::database
        ReportDB[("🗄️ Report DB")]:::database
        AlertDB[("🗄️ Alert DB")]:::database
    end

    %% Service to DB connections
    AuthSvc ===> AuthDB
    ProductSvc ===> ProductDB
    WarehouseSvc ===> WarehouseDB
    MovementSvc ===> MovementDB
    SupplierSvc ===> SupplierDB
    POSvc ===> PODB
    ReportSvc ===> ReportDB
    AlertSvc ===> AlertDB

```

### Key Architectural Characteristics
* **API Gateway Pattern:** All external traffic is routed through the Ocelot API Gateway which forwards requests to specific services based on URL prefixes.
* **Database-per-Service:** Each microservice manages its own separate SQL Server database, ensuring loose coupling and isolated data domains.
* **Synchronous Communication:** Services communicate with each other over HTTP using `HttpClient` instead of an asynchronous message broker (like RabbitMQ or Kafka).
* **Background Processing:** The `AlertService` contains a background worker that periodically polls other services to check for low stock thresholds.

Each service follows a consistent layered structure:

```
ServiceName/
├── Controllers/      # HTTP endpoints
├── Services/         # Business logic
├── Repositories/     # Data access layer
├── Entities/         # EF Core models
├── DTOs/             # Request / response models
├── Data/             # DbContext
├── Middleware/       # Global exception handling
└── Migrations/       # EF Core migrations
```

---

## Microservice Internal Workflows (Service Flows)

To visualize how the microservices communicate with one another to accomplish business goals, here are the Sequence Diagrams for the two most complex cross-service flows in the system.

### 1. Flow 1: Receiving Goods for a Purchase Order
When a warehouse manager receives items for an approved Purchase Order, the system must update the PO, update the actual warehouse stock, and log a stock movement record. This involves synchronous communication across **three different microservices**.

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant API_GW as API Gateway
    participant POSvc as Purchase Order Service
    participant PODB as Purchase DB
    participant WhSvc as Warehouse Service
    participant WhDB as Warehouse DB
    participant MovSvc as Stock Movement Service
    participant MovDB as Movement DB

    Client->>API_GW: POST /api/purchase-orders/{id}/receive
    API_GW->>POSvc: Route request to POSvc
    
    POSvc->>PODB: Begin SQL Transaction
    POSvc->>PODB: Fetch PO & Line Items
    PODB-->>POSvc: Return PO data
    
    loop For Each Received Item
        %% Step 5
        POSvc->>WhSvc: POST /api/stock/update
        Note over POSvc,WhSvc: Sends: productId, warehouseId, quantity
        
        WhSvc->>WhDB: Increment Available Quantity
        
        %% Step 7
        WhSvc->>MovSvc: POST /api/movements
        Note over WhSvc,MovSvc: Logs a "STOCK_IN" movement
        
        MovSvc->>MovDB: Save Movement Log
        MovDB-->>MovSvc: Saved
        MovSvc-->>WhSvc: 201 Created
        
        WhSvc-->>POSvc: 200 OK (Stock Updated)
        
        POSvc->>PODB: Update POLineItem ReceivedQty
    end
    
    alt All Items Fully Received?
        POSvc->>PODB: Update PO Status to "RECEIVED"
    else Partially Received
        POSvc->>PODB: Update PO Status to "PARTIALLY_RECEIVED"
    end
    
    POSvc->>PODB: Commit SQL Transaction
    POSvc-->>API_GW: 200 OK
    API_GW-->>Client: Success Response
```

### 2. Flow 2: Low Stock Alert Background Job
The `AlertService` contains a background worker (`LowStockWorker.cs`) that runs periodically to check if any products have dipped below their defined reorder levels.

```mermaid
sequenceDiagram
    autonumber
    participant Worker as AlertService (Worker)
    participant WhSvc as Warehouse Service
    participant ProdSvc as Product Service
    participant AlertDB as Alert DB

    loop Triggered by Timer
        Worker->>WhSvc: GET /api/stock/all
        WhSvc-->>Worker: Return list of all StockLevels
        
        loop For each StockLevel
            Worker->>ProdSvc: GET /api/products/{id}
            ProdSvc-->>Worker: Return Product details
            
            opt If AvailableQuantity <= Product.ReorderLevel
                Worker->>WhSvc: GET /api/warehouses/{id}
                WhSvc-->>Worker: Return Warehouse details
                Note over Worker,WhSvc: Needed to find the ManagerId
                
                Worker->>AlertDB: Create "LOW_STOCK" Alert
                Note over Worker,AlertDB: RecipientId = Warehouse.ManagerId
                AlertDB-->>Worker: Alert Saved
            end
        end
    end
```

---

## Data Architecture (ER Diagram)

This section provides a logical Entity-Relationship (ER) diagram for the StockPro system. 
**Note:** Because StockPro uses a Database-per-Service architecture, these entities actually live in physically separate SQL Server databases. The relationships mapped here (like `PurchaseOrder` -> `Supplier`) are "logical" relationships that your microservices handle programmatically via IDs.

```mermaid
erDiagram
    %% Entities
    APP_USER {
        uniqueidentifier UserId PK
        string FullName
        string Email
        string PasswordHash
        string Phone
        string Role
        string Department
        boolean IsActive
        datetime CreatedAt
        datetime LastLoginAt
    }

    PRODUCT {
        uniqueidentifier ProductId PK
        string Sku
        string Name
        string Description
        string Category
        string Brand
        string UnitOfMeasure
        float CostPrice
        float SellingPrice
        int ReorderLevel
        int MaxStockLevel
        int LeadTimeDays
        string ImageUrl
        boolean IsActive
        string Barcode
    }

    SUPPLIER {
        int SupplierId PK
        string Name
        string ContactPerson
        string Email
        string Phone
        string Address
        string City
        string Country
        string TaxId
        string PaymentTerms
        int LeadTimeDays
        float Rating
        boolean IsActive
    }

    WAREHOUSE {
        int WarehouseId PK
        string Name
        string Location
        string Address
        int ManagerId FK
        int Capacity
        int UsedCapacity
        boolean IsActive
        string Phone
        datetime CreatedAt
    }

    STOCK_LEVEL {
        int StockId PK
        int WarehouseId FK
        uniqueidentifier ProductId FK
        int Quantity
        int ReservedQuantity
        string Location
        datetime LastUpdated
    }

    STOCK_MOVEMENT {
        int MovementId PK
        uniqueidentifier ProductId FK
        int WarehouseId FK
        string MovementType
        int Quantity
        string ReferenceType
        int ReferenceId
        float UnitCost
        uniqueidentifier PerformedBy FK
        string Notes
        datetime MovementDate
        int BalanceAfter
    }

    PURCHASE_ORDER {
        int PoId PK
        int SupplierId FK
        int WarehouseId FK
        uniqueidentifier CreatedById FK
        string Status
        float TotalAmount
        datetime OrderDate
        datetime ExpectedDate
        datetime ReceivedDate
        string Notes
        string ReferenceNumber
    }

    PO_LINE_ITEM {
        int LineItemId PK
        int PoId FK
        uniqueidentifier ProductId FK
        int Quantity
        float UnitCost
        float TotalCost
        int ReceivedQty
    }

    INVENTORY_SNAPSHOT {
        int SnapshotId PK
        int WarehouseId FK
        uniqueidentifier ProductId FK
        int Quantity
        float StockValue
        date SnapshotDate
        datetime CreatedAt
    }

    ALERT {
        int AlertId PK
        int RecipientId FK
        string Type
        string Severity
        string Title
        string Message
        uniqueidentifier RelatedProductId FK
        int RelatedWarehouseId FK
        string Channel
        boolean IsRead
        boolean IsAcknowledged
        datetime CreatedAt
    }

    %% Logical Relationships
    PURCHASE_ORDER ||--o{ PO_LINE_ITEM : "contains"
    SUPPLIER ||--o{ PURCHASE_ORDER : "supplies"
    WAREHOUSE ||--o{ PURCHASE_ORDER : "receives"
    APP_USER ||--o{ PURCHASE_ORDER : "creates"
    
    PRODUCT ||--o{ PO_LINE_ITEM : "ordered in"
    PRODUCT ||--o{ STOCK_LEVEL : "stocked as"
    WAREHOUSE ||--o{ STOCK_LEVEL : "holds"
    
    PRODUCT ||--o{ STOCK_MOVEMENT : "moves"
    WAREHOUSE ||--o{ STOCK_MOVEMENT : "tracked in"
    APP_USER ||--o{ STOCK_MOVEMENT : "performed by"
    
    PRODUCT ||--o{ INVENTORY_SNAPSHOT : "recorded in"
    WAREHOUSE ||--o{ INVENTORY_SNAPSHOT : "recorded for"
    
    PRODUCT ||--o{ ALERT : "triggers"
    WAREHOUSE ||--o{ ALERT : "triggers"
```

### Schema Observations & Potential Bugs Noticed
While generating this diagram from your Entity classes, I noticed a few **data type mismatches** for foreign keys across microservices that might cause bugs when you try to join data:
1. `AppUser.UserId` is a `Guid`, but `Warehouse.ManagerId` is an `int`.
2. `AppUser.UserId` is a `Guid`, but `Alert.RecipientId` is an `int`.

You might want to update `ManagerId` and `RecipientId` to be `Guid` properties so they properly map to the Auth Service's `UserId`.

---

## Services

| Service | Port | Responsibility |
|---|---|---|
| **ApiGateway** | 5000 | Single entry point, routes all requests via Ocelot |
| **AuthService** | 5119 | User authentication, JWT token issuance, role management |
| **ProductService** | 5212 | Product catalog CRUD |
| **WarehouseService** | 5019 | Warehouse management and stock levels |
| **StockMovementService** | 5121 | Track stock movements (in/out/transfer) |
| **SupplierService** | 5230 | Supplier management |
| **PurchaseOrderService** | 5189 | Purchase order lifecycle |
| **AlertService** | 5245 | Low-stock and threshold alerts |
| **ReportService** | 5144 | Reporting and analytics |

---

## Tech Stack

- **Runtime**: .NET 8 / ASP.NET Core
- **ORM**: Entity Framework Core
- **Database**: SQL Server (SQL Express for local dev)
- **API Gateway**: Ocelot
- **Auth**: JWT Bearer tokens
- **API Docs**: Swagger / OpenAPI (per service)

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server Express
- Git

---

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd StockPro
```

### 2. Configure connection strings

Each service has its own `appsettings.json`. Update the `DefaultConnection` string to point to your SQL Server instance.

**Example** (`AuthService/appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=AuthDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_HERE",
    "Issuer": "StockPro",
    "Audience": "StockProUsers"
  }
}
```

Repeat for each service, using a separate database name per service (e.g., `ProductDB`, `WarehouseDB`, etc.).

### 3. Run database migrations

Run migrations for each service from the solution root:

```bash
dotnet ef database update --project AuthService
dotnet ef database update --project ProductService
dotnet ef database update --project WarehouseService
dotnet ef database update --project StockMovementService
dotnet ef database update --project SupplierService
dotnet ef database update --project PurchaseOrderService
dotnet ef database update --project AlertService
dotnet ef database update --project ReportService
```

### 4. Start all services

Open a terminal for each service and run:

```bash
dotnet run --project ApiGateway
dotnet run --project AuthService
dotnet run --project ProductService
dotnet run --project WarehouseService
dotnet run --project StockMovementService
dotnet run --project SupplierService
dotnet run --project PurchaseOrderService
dotnet run --project AlertService
dotnet run --project ReportService
```

All client traffic should be routed through the gateway at `http://localhost:5000`.

---

## API Documentation

Each service exposes a Swagger UI at:

```
http://localhost:{PORT}/swagger
```

All secured endpoints require a **Bearer JWT token** in the `Authorization` header:

```
Authorization: Bearer <your_token>
```

Obtain a token via `POST /api/auth/login` through the gateway.

---

## Authentication & Authorization

- JWT tokens are issued by **AuthService** on successful login.
- Tokens include the user's **role** (`Admin` / `User`).
- Every downstream service validates the JWT independently.
- Role-based access is enforced at the controller level via `[Authorize(Roles = "...")]`.

> **Note:** User registration is restricted to administrators only. New accounts must be created by an existing admin.

---

## Testing

Each service has a corresponding test project (e.g., `AuthService.Tests`, `ProductService.Tests`).

Run all tests from the solution root:

```bash
dotnet test
```

---

## Project Structure

```
StockPro/
├── ApiGateway/
├── AuthService/
├── AuthService.Tests/
├── ProductService/
├── ProductService.Tests/
├── WarehouseService/
├── WarehouseService.Tests/
├── StockMovementService/
├── StockMovementService.Tests/
├── SupplierService/
├── SupplierService.Tests/
├── PurchaseOrderService/
├── PurchaseOrderService.Tests/
├── AlertService/
├── AlertService.Tests/
├── ReportService/
├── ReportService.Tests/
└── StockPro.sln
```

---
