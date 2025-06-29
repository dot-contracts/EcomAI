# EcomVideo AI Backend

A .NET 8 Web API for AI-powered video generation from product data, built with Clean Architecture principles.

## Features

- **Text-to-Video Generation**: Convert text prompts into promotional videos using Freepik API
- **Clean Architecture**: Domain-driven design with clear separation of concerns
- **PostgreSQL Database**: Robust relational database with EF Core
- **Real-time Updates**: Video processing status updates
- **RESTful API**: Well-documented API endpoints
- **Health Checks**: Built-in health monitoring
- **Comprehensive Logging**: Structured logging with Serilog

## Technology Stack

- **.NET 8** - Web API framework
- **PostgreSQL** - Primary database
- **Entity Framework Core** - ORM with Code First approach
- **Freepik API** - AI video generation service
- **Redis** - Caching and session storage
- **Serilog** - Structured logging
- **FluentValidation** - Request validation
- **Swagger/OpenAPI** - API documentation
- **Docker** - Containerization for development

## Prerequisites

- .NET 8 SDK
- Docker and Docker Compose
- PostgreSQL (via Docker or local installation)
- Freepik API key

## Quick Start

### 1. Clone and Setup

```bash
git clone <repository-url>
cd EcomVideoAI.Backend
```

### 2. Start Database Services

```bash
# Start PostgreSQL and Redis using Docker Compose
docker-compose -f docker-compose.dev.yml up -d

# Verify services are running
docker-compose -f docker-compose.dev.yml ps
```

This will start:
- **PostgreSQL** on port 5432
- **Redis** on port 6379  
- **pgAdmin** on port 8080 (admin@ecomvideoai.com / admin123)

### 3. Configure API Keys

Update `appsettings.Development.json`:

```json
{
  "FreepikApi": {
    "ApiKey": "your-actual-freepik-api-key-here",
    "BaseUrl": "https://api.freepik.com"
  }
}
```

### 4. Install Dependencies

```bash
cd src/EcomVideoAI.API
dotnet restore
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at:
- **Swagger UI**: https://localhost:7001
- **API Base**: https://localhost:7001/api
- **Health Check**: https://localhost:7001/health

## Database Management

### Migrations

The application uses Entity Framework Code First migrations:

```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project src/EcomVideoAI.Infrastructure --startup-project src/EcomVideoAI.API

# Update database
dotnet ef database update --project src/EcomVideoAI.Infrastructure --startup-project src/EcomVideoAI.API

# Remove last migration
dotnet ef migrations remove --project src/EcomVideoAI.Infrastructure --startup-project src/EcomVideoAI.API
```

### Database Access

- **pgAdmin**: http://localhost:8080
  - Email: admin@ecomvideoai.com
  - Password: admin123
  - Server: postgres (container name)
  - Database: ecomvideoai_dev
  - Username: postgres
  - Password: password

## API Endpoints

### Video Generation

```http
POST /api/video/create-from-text
Content-Type: application/json

{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Product Video",
  "description": "Promotional video for our product",
  "textPrompt": "A stylish hoodie with urban background, energetic music",
  "negativePrompt": "blurry, low quality",
  "resolution": "HD_720p",
  "duration": 5
}
```

### Get Video Status

```http
GET /api/video/{id}/status
```

### Get User Videos

```http
GET /api/video/user/{userId}?page=1&pageSize=10
```

## Project Structure

```
EcomVideoAI.Backend/
├── src/
│   ├── EcomVideoAI.API/              # Web API Layer
│   ├── EcomVideoAI.Application/      # Use Cases & Business Logic
│   ├── EcomVideoAI.Domain/           # Core Business Logic & Entities
│   ├── EcomVideoAI.Infrastructure/   # External Services & Data Access
│   └── EcomVideoAI.Shared/           # Cross-cutting Concerns
├── tests/                            # Test Projects
├── docker-compose.dev.yml            # Development Services
└── README.md
```

## Configuration

### Environment Variables

For production, consider using environment variables:

```bash
export ConnectionStrings__DefaultConnection="Host=prod-host;Database=ecomvideoai;Username=user;Password=pass"
export FreepikApi__ApiKey="your-production-api-key"
export Jwt__SecretKey="your-production-secret-key"
```

### Configuration Files

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production settings

## Development Workflow

### 1. Video Generation Flow

1. Client sends text-to-video request
2. API creates Video entity in database
3. Background service calls Freepik API for image generation
4. Once image is ready, calls Freepik API for video generation
5. Polls for completion and updates database
6. Client can check status via polling or WebSocket

### 2. Adding New Features

1. **Domain Layer**: Add entities, value objects, or domain services
2. **Application Layer**: Create use cases and DTOs
3. **Infrastructure Layer**: Implement external service integrations
4. **API Layer**: Add controllers and endpoints

### 3. Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/EcomVideoAI.UnitTests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Troubleshooting

### Common Issues

1. **Database Connection Failed**
   ```bash
   # Check if PostgreSQL is running
   docker-compose -f docker-compose.dev.yml ps postgres
   
   # Check logs
   docker-compose -f docker-compose.dev.yml logs postgres
   ```

2. **Migration Issues**
   ```bash
   # Reset database (development only)
   dotnet ef database drop --project src/EcomVideoAI.Infrastructure --startup-project src/EcomVideoAI.API
   dotnet ef database update --project src/EcomVideoAI.Infrastructure --startup-project src/EcomVideoAI.API
   ```

3. **Package Restore Issues**
   ```bash
   # Clear NuGet cache
   dotnet nuget locals all --clear
   dotnet restore
   ```

### Logs

Application logs are written to:
- Console output
- `logs/ecomvideoai-dev-{date}.txt`

## Contributing

1. Follow Clean Architecture principles
2. Write unit tests for business logic
3. Use FluentValidation for input validation
4. Follow RESTful API conventions
5. Update documentation for new endpoints

## License

[Your License Here]