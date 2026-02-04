# 💊 Pharmacy Stock Management System

A comprehensive pharmacy inventory management system built with **ASP.NET Core 10.0** following **Clean Architecture** principles, featuring real-time notifications, role-based access control, and multi-database support.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=csharp)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?style=flat)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

## 🎯 Project Overview

This system provides complete pharmacy inventory management capabilities including medicine tracking, stock movements, expiry monitoring, supplier management, and real-time dashboard updates.

## ✨ Key Features

- 🔐 **JWT Authentication & Authorization** - Secure token-based authentication with refresh tokens
- 👥 **Role-Based Access Control (RBAC)** - Dynamic permission-based authorization system
- 📦 **Inventory Management** - Track medicines, stock levels, and movements
- ⚠️ **Expiry Monitoring** - Automated alerts for expiring medicines with configurable rules
- 📊 **Real-time Dashboard** - Live updates using SignalR for stock changes and notifications
- 🔔 **Notification System** - In-app notifications for critical events
- 📧 **Email Notifications** - SMTP-based email service for user notifications
- 🏢 **Supplier Management** - Track suppliers and their medicine catalog
- 📝 **Audit Trail** - Complete tracking of who changed what and when
- 🗄️ **Multi-Database Support** - SQL Server and PostgreSQL compatibility
- ⚡ **Redis Caching** - High-performance caching layer for frequently accessed data
- 📋 **Comprehensive Logging** - Structured logging with Serilog

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
PharmacyStock.Backend/
├── PharmacyStock.API/              # Presentation Layer
│   ├── Controllers/                # API endpoints
│   ├── Hubs/                       # SignalR hubs for real-time communication
│   ├── Middleware/                 # Global exception & performance middleware
│   └── Services/                   # API-specific services
│
├── PharmacyStock.Application/      # Application Layer
│   ├── DTOs/                       # Data Transfer Objects
│   ├── Interfaces/                 # Service contracts
│   ├── Services/                   # Business logic implementation
│   ├── Mappings/                   # AutoMapper profiles
│   └── Utilities/                  # Helper utilities
│
├── PharmacyStock.Domain/           # Domain Layer
│   ├── Entities/                   # Domain models
│   ├── Enums/                      # Enumerations
│   ├── Constants/                  # Domain constants
│   └── Interfaces/                 # Domain contracts
│
└── PharmacyStock.Infrastructure/   # Infrastructure Layer
    ├── Persistence/                # Database context & configurations
    ├── Migrations/                 # EF Core migrations (SQL Server & PostgreSQL)
    └── Services/                   # Infrastructure services
```

## 🛠️ Technology Stack

### Core Framework
- **ASP.NET Core 10.0** - Latest .NET framework
- **C# 12.0** - Modern C# with nullable reference types enabled
- **Entity Framework Core 10.0** - ORM for database operations

### Database
- **SQL Server 2022** - Primary database option
- **PostgreSQL 17** - Alternative database provider
- **Redis** - Distributed caching

### Authentication & Security
- **JWT Bearer Authentication** - Token-based authentication
- **BCrypt.Net** - Password hashing
- **Microsoft.IdentityModel.Tokens** - Token validation

### Real-time Communication
- **SignalR** - Real-time web functionality for dashboard updates

### Logging & Monitoring
- **Serilog** - Structured logging
  - Console sink
  - File sink with rolling intervals
  - Request logging middleware

### API Documentation
- **Swagger/OpenAPI** - Interactive API documentation

### Email
- **System.Net.Mail** - SMTP email service

### Utilities
- **AutoMapper 13.0** - Object-to-object mapping



## 📋 Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server 2022](https://www.microsoft.com/sql-server) or [PostgreSQL 17](https://www.postgresql.org/)
- [Redis](https://redis.io/) (optional, for caching)

## 🚀 Getting Started

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd PharmacyStock/Backend
   ```

2. **Configure the database**
   
   Edit `PharmacyStock.API/appsettings.json`:
   ```json
   {
     "Provider": "SqlServer",  // or "PostgreSQL"
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=PharmacyStockDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true;",
       "PostgresConnection": "Host=localhost;Database=PharmacyStockDb;Username=postgres;Password=YourPassword"
     }
   }
   ```

3. **Run the application**
   ```bash
   cd PharmacyStock.API
   dotnet run
   ```
   
   > **Note**: Database migrations are automatically applied on startup, and default users are seeded.

4. **Access the API**
   - API: `https://localhost:7000` or `http://localhost:5041`
   - Swagger UI: `https://localhost:7000/swagger`

## 🔑 Default Credentials

After initial setup, the system seeds default users:

- **Admin User**
  - Username: `admin`
  - Password: `Admin@123`
  - Role: Administrator with full access

- **Pharmacist User**
  - Username: `pharmacist`
  - Password: `Pharmacist@123`
  - Role: Pharmacist with limited access

> ⚠️ **Important**: Change these passwords in production!

## 📡 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/refresh-token` - Refresh access token

### Users & Roles
- `GET /api/users` - List all users
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `GET /api/roles` - List all roles
- `POST /api/roles` - Create new role

### Medicines
- `GET /api/medicines` - List all medicines
- `GET /api/medicines/{id}` - Get medicine details
- `POST /api/medicines` - Create new medicine
- `PUT /api/medicines/{id}` - Update medicine
- `DELETE /api/medicines/{id}` - Delete medicine

### Inventory
- `GET /api/inventory` - Get current stock levels
- `GET /api/inventory/low-stock` - Get low stock items
- `GET /api/inventory/expiring-soon` - Get expiring medicines

### Stock Operations
- `POST /api/stock-operations/receive` - Receive stock
- `POST /api/stock-operations/dispense` - Dispense medicine
- `POST /api/stock-operations/adjust` - Adjust stock levels
- `POST /api/stock-operations/transfer` - Transfer between locations

### Dashboard
- `GET /api/dashboard/summary` - Get dashboard statistics
- `GET /api/dashboard/recent-activities` - Get recent stock movements

### Suppliers
- `GET /api/suppliers` - List all suppliers
- `POST /api/suppliers` - Create new supplier
- `PUT /api/suppliers/{id}` - Update supplier

### Notifications
- `GET /api/notifications` - Get user notifications
- `PUT /api/notifications/{id}/read` - Mark notification as read

For complete API documentation, visit the Swagger UI at `/swagger`.

## 🔌 SignalR Hubs

### Dashboard Hub (`/hubs/dashboard`)
Real-time updates for:
- Stock level changes
- New notifications
- Expiry alerts
- Dashboard statistics updates

## 🗄️ Database Migrations

This project supports both SQL Server and PostgreSQL with separate migration contexts.

### SQL Server
```bash
dotnet ef migrations add MigrationName --context AppDbContext --output-dir Migrations/SqlServer --project PharmacyStock.Infrastructure --startup-project PharmacyStock.API
dotnet ef database update --context AppDbContext --project PharmacyStock.Infrastructure --startup-project PharmacyStock.API
```

### PostgreSQL
```bash
dotnet ef migrations add MigrationName --context AppDbContextPostgres --output-dir Migrations/Postgres --project PharmacyStock.Infrastructure --startup-project PharmacyStock.API
dotnet ef database update --context AppDbContextPostgres --project PharmacyStock.Infrastructure --startup-project PharmacyStock.API
```

## 📊 Project Statistics

- **Total Projects**: 4 (API, Application, Domain, Infrastructure)
- **Controllers**: 12
- **Architecture Pattern**: Clean Architecture
- **Design Patterns**: Repository, Unit of Work, CQRS-inspired service layer
- **Database Providers**: 2 (SQL Server, PostgreSQL)

## 🔒 Security Features

- ✅ JWT token authentication with refresh tokens
- ✅ Cookie-based token storage
- ✅ Permission-based authorization
- ✅ Password hashing with BCrypt
- ✅ CORS configuration
- ✅ HTTPS redirection
- ✅ SQL injection protection via EF Core
- ✅ Global exception handling middleware

## 🎯 Performance Features

- ⚡ Redis distributed caching
- ⚡ Performance monitoring middleware
- ⚡ Async/await throughout
- ⚡ Database query optimization
- ⚡ Connection pooling

## 📝 Logging

Logs are written to:
- **Console** - For development and Docker logs
- **File** - `Logs/log-YYYYMMDD.txt` with daily rolling

Log levels:
- `Information` - Default level
- `Warning` - Microsoft and System components
- `Error` - Exceptions and errors
- `Fatal` - Application crashes


## 📦 Building for Production

```bash
# Publish the application
dotnet publish -c Release -o ./publish
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👨‍💻 Author

**Si Thu Kyaw**

## 🙏 Acknowledgments

- Clean Architecture principles by Robert C. Martin
- ASP.NET Core documentation and community
- Entity Framework Core team
- All open-source libraries used in this project
