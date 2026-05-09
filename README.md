# StockPro — Backend API

> A microservices-based inventory management system built with **ASP.NET Core**, **Entity Framework Core**, **SQL Server**, and an **Ocelot API Gateway**.

---

## Architecture Overview

StockPro is structured as a collection of independent microservices, each owning its own database and exposed through a unified API Gateway.

```
Client / Frontend
       │
       ▼
  API Gateway (Port 5000)          ← Ocelot
       │
       ├── AuthService          (Port 5119)
       ├── ProductService       (Port 5212)
       ├── WarehouseService     (Port 5019)
       ├── StockMovementService (Port 5121)
       ├── SupplierService      (Port 5230)
       ├── PurchaseOrderService (Port 5189)
       ├── AlertService         (Port 5245)
       └── ReportService        (Port 5144)
```

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

## License

This project is licensed under the **MIT License**.
