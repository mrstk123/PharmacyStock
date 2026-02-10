# 💊 Pharmacy Stock Management System

A comprehensive pharmacy inventory management system built with **ASP.NET Core 10.0** following **Clean Architecture** principles, featuring real-time notifications, role-based access control, and multi-database support.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat&logo=csharp)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?style=flat)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

## 🌐 Live Demo

🚀 **Live Application:** [https://pharmacystock.vercel.app/](https://pharmacystock.vercel.app/)

### 🔑 Demo Credentials

| Role | Username | Password |
|------|----------|----------|
| **Admin** | `admin` | `Admin@123` |
| **Pharmacist** | `pharmacist` | `Pharmacist@123` |

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
- 🐳 **Docker Deployment** - Dynamic database provider selection with Docker Compose profiles
- ⚡ **Redis Caching** - High-performance caching layer for frequently accessed data
- 📋 **Comprehensive Logging** - Structured logging with Serilog
- 🧪 **Robust Testing Suite** - Full unit and integration test coverage with xUnit

### 🔗 Related Repositories

- [**Frontend Repository (Angular)**](https://github.com/mrstk123/PharmacyStock.Client)

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
- **SQL Server 2022** - Enterprise database option
- **PostgreSQL 17** - Open-source database (Default)
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

### Deployment
- **Docker** - Containerization platform
- **Docker Compose** - Multi-container orchestration with profile-based database selection


## 📋 Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server 2022](https://www.microsoft.com/sql-server) or [PostgreSQL 17](https://www.postgresql.org/)
- [Redis](https://redis.io/)

## 🚀 Getting Started

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd PharmacyStock
   ```

2. **Configure the database**
   
   Edit `PharmacyStock.API/appsettings.json`:
   ```json
   {
     "Provider": "PostgreSQL",  // or "SqlServer" 
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=PharmacyStockDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true;",
       "PostgresConnection": "Host=localhost;Database=PharmacyStockDb;Username=postgres;Password=YourPassword",
       "RedisConnection": "localhost:6379"
     }
   }
   ```

3. **Run the application**
   ```bash
   cd PharmacyStock.API
   dotnet run
   ```

4. **Access the API**
   - API: `https://localhost:7000` or `http://localhost:5041`
   - Swagger UI: `https://localhost:7000/swagger`

5. **Login with Default Credentials**

   The system seeds default users for initial access:

   - **Admin User**
     - Username: `admin`
     - Password: `Admin@123`
     - Role: Administrator with full access

   - **Pharmacist User**
     - Username: `pharmacist`
     - Password: `Pharmacist@123`
     - Role: Pharmacist with limited access
## 🐳 Docker Deployment

The application supports Docker deployment with **dynamic database provider selection** using Docker Compose profiles.

### Prerequisites
- [Docker](https://www.docker.com/get-started)
- [Docker Compose](https://docs.docker.com/compose/install/)

### Quick Start with Docker

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd PharmacyStock
   ```

2. **Start with PostgreSQL (default)**
   ```bash
   docker-compose --profile postgres up -d
   ```

3. **Access the application**
   - API: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`

### Environment Configuration

Create a `.env` file for custom configuration:

```bash
cp .env.example .env
```

Edit `.env` to customize database provider and settings:

```env
# Change database provider (PostgreSQL or SqlServer)
DB_PROVIDER=PostgreSQL

# Customize passwords and ports as needed
POSTGRES_PASSWORD=YourSecurePassword
MSSQL_PASSWORD=YourSecurePassword
API_PORT=5000
ASPNETCORE_ENVIRONMENT=Development
CLIENT_APP_URL=http://localhost:4200
```

**Important:** Use the profile that matches your `DB_PROVIDER`:
```bash
# If DB_PROVIDER=PostgreSQL
docker-compose --profile postgres up -d

# If DB_PROVIDER=SqlServer
docker-compose --profile mssql up -d
```

### Switching Database Providers

To switch from one database to another:

```bash
# Stop and remove current containers
docker-compose down -v

# Update .env file (edit DB_PROVIDER line)
sed -i '' 's/DB_PROVIDER=.*/DB_PROVIDER=PostgreSQL/' .env

# Start with new profile
docker-compose --profile postgres up -d
```

### Docker Commands

```bash
# View logs
docker-compose logs -f pharmacy-api

# Check service health
docker-compose ps

# Stop all services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v

# Rebuild API image
docker-compose build pharmacy-api
```

## 📡 API Documentation

Comprehensive API documentation is available via **Swagger/OpenAPI**.

- **Swagger UI**: Visit `/swagger` (e.g., `https://localhost:7000/swagger`) when running the app to explore and test endpoints.

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

## 🧪 Testing

The solution includes comprehensive unit and integration tests following the Clean Architecture structure:

### Test Projects

- **PharmacyStock.Application.Tests**
  - **Type**: Unit Tests
  - **Focus**: Business logic, Services, Validators, Mappings
  - **Tools**: xUnit, Moq, FluentAssertions
  - **Coverage**: High coverage of the service methods

- **PharmacyStock.API.Tests**
  - **Type**: Integration Tests
  - **Focus**: Controllers, Middleware, Auth Flow, HTTP Responses
  - **Tools**: WebApplicationFactory (TestServer), FakePolicyEvaluator
  - **Database**: In-Memory Database (isolated per test class)

- **PharmacyStock.Infrastructure.Tests**
  - **Type**: Infrastructure Tests
  - **Focus**: DateServices, External Integrations
  - **Tools**: xUnit, FluentAssertions

### Running Tests

Execute all tests from the backend directory:

```bash
cd PharmacyStock.Backend
dotnet test
```

Or run a specific project:

```bash
dotnet test PharmacyStock.Application.Tests
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
