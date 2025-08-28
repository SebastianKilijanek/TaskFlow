# TaskFlow Project Scripts & Commands

## 📋 Project Overview
TaskFlow is a .NET 8.0 Clean Architecture project with the following structure:
- **TaskFlow.API** - ASP.NET Core Web API
- **TaskFlow.Application** - Business logic layer (CQRS/MediatR)
- **TaskFlow.Domain** - Domain entities and interfaces
- **TaskFlow.Infrastructure** - Data access and external services
- **TaskFlow.Tests** - Unit and integration tests

## 🏗️ Build Commands

### Basic Build Operations
```bash
# Restore NuGet packages
dotnet restore

# Build entire solution
dotnet build

# Build specific project
dotnet build TaskFlow.API

# Clean build artifacts
dotnet clean

# Build in Release mode
dotnet build --configuration Release
```

## 🚀 Run Commands

### Development Server
```bash
# Run API server (Development mode)
dotnet run --project TaskFlow.API

# Run with hot reload (auto-restart on file changes)
dotnet watch run --project TaskFlow.API

# Run with specific launch profile
dotnet run --project TaskFlow.API --launch-profile https
```

### Available Endpoints (Development)
- HTTP: `http://localhost:5177`
- HTTPS: `https://localhost:7112`
- Swagger UI: `http://localhost:5177/swagger`

## 🧪 Testing Commands

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests for specific project
dotnet test TaskFlow.Tests

# Run tests with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"
```

**Note**: Currently there are test compilation errors that need to be fixed.

## 🗄️ Database Commands (Entity Framework)

### Migrations
```bash
# Add new migration
dotnet ef migrations add <MigrationName> \
  --project TaskFlow.Infrastructure \
  --startup-project TaskFlow.API

# Update database to latest migration
dotnet ef database update \
  --project TaskFlow.Infrastructure \
  --startup-project TaskFlow.API

# List all migrations
dotnet ef migrations list \
  --project TaskFlow.Infrastructure \
  --startup-project TaskFlow.API

# Remove last migration (if not applied)
dotnet ef migrations remove \
  --project TaskFlow.Infrastructure \
  --startup-project TaskFlow.API

# Generate SQL script for migration
dotnet ef migrations script \
  --project TaskFlow.Infrastructure \
  --startup-project TaskFlow.API
```

### Database Operations
```bash
# Drop database
dotnet ef database drop \
  --project TaskFlow.Infrastructure \
  --startup-project TaskFlow.API

# Update to specific migration
dotnet ef database update <MigrationName> \
  --project TaskFlow.Infrastructure \
  --startup-project TaskFlow.API
```

## 🐳 Docker Commands

### Building Images
```bash
# Build Docker image
docker build -t taskflow .

# Build with specific tag
docker build -t taskflow:v1.0 .

# Build development image
docker build --target dev -t taskflow:dev .
```

### Running with Docker Compose
```bash
# Start all services (API + PostgreSQL)
docker-compose up

# Start in background/detached mode
docker-compose up -d

# Stop all services
docker-compose down

# View logs
docker-compose logs
docker-compose logs api
docker-compose logs db

# Rebuild and start
docker-compose up --build
```

### Individual Docker Commands
```bash
# Run PostgreSQL database only
docker run -d \
  --name taskflow-db \
  -e POSTGRES_DB=taskflow \
  -e POSTGRES_USER=taskflow \
  -e POSTGRES_PASSWORD=devpass \
  -p 5432:5432 \
  postgres:15

# Run API container (after building)
docker run -p 5000:8080 \
  -e ConnectionStrings__Default="Host=host.docker.internal;Database=taskflow;Username=taskflow;Password=devpass" \
  taskflow
```

## 📦 Package Management

### NuGet Operations
```bash
# Add package to specific project
dotnet add TaskFlow.API package <PackageName>

# Remove package
dotnet remove TaskFlow.API package <PackageName>

# List packages for project
dotnet list TaskFlow.API package

# Update packages
dotnet restore --force
```

## 🔧 Development Tools

### Code Analysis
```bash
# Format code (if .editorconfig is present)
dotnet format

# Analyze code for issues
dotnet build --verbosity normal
```

### Project Information
```bash
# List all projects in solution
dotnet sln list

# Show project dependencies
dotnet list reference
dotnet list TaskFlow.API reference
```

## 🌐 API Testing

### Using the HTTP File
The project includes `TaskFlow.API.http` for testing API endpoints:
- Open in VS Code with REST Client extension
- Or use in Visual Studio/Rider

### Manual Testing
```bash
# Test health endpoint (if available)
curl http://localhost:5177/health

# Test Swagger UI
open http://localhost:5177/swagger
```

## 📝 Configuration Files

### Application Settings
- `appsettings.json` - Production configuration
- `appsettings.Development.json` - Development configuration
- `launchSettings.json` - VS launch profiles

### Key Configuration Sections
- **ConnectionStrings**: Database connection
- **Jwt**: JWT authentication settings
- **EmailOptions**: SMTP configuration for emails
- **Logging**: Serilog configuration

## 🚦 Environment Setup

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL (or use Docker)
- Docker & Docker Compose (optional)

### Quick Start
```bash
# 1. Restore packages
dotnet restore

# 2. Start database (Docker)
docker-compose up db -d

# 3. Run migrations (if needed)
dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.API

# 4. Start API
dotnet run --project TaskFlow.API

# 5. Open Swagger UI
open http://localhost:5177/swagger
```

## 🏷️ Common Development Workflows

### Feature Development
```bash
# 1. Pull latest changes
git pull

# 2. Create feature branch
git checkout -b feature/new-feature

# 3. Make changes and test
dotnet watch run --project TaskFlow.API

# 4. Run tests
dotnet test TaskFlow.Tests

# 5. Build for production
dotnet build --configuration Release
```

### Database Changes
```bash
# 1. Modify entities in TaskFlow.Domain
# 2. Add migration
dotnet ef migrations add AddNewFeature --project TaskFlow.Infrastructure --startup-project TaskFlow.API

# 3. Update database
dotnet ef database update --project TaskFlow.Infrastructure --startup-project TaskFlow.API

# 4. Test changes
dotnet run --project TaskFlow.API
```

## 🎯 Project Architecture Notes

This project follows Clean Architecture principles:
- **Domain**: Core business entities and rules
- **Application**: Use cases and business logic (CQRS with MediatR)
- **Infrastructure**: External concerns (database, email, etc.)
- **API**: Presentation layer (controllers, middleware)

The build system uses standard .NET tooling with MSBuild, so all standard `dotnet` commands work as expected.