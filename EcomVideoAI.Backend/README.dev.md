# EcomVideo AI Backend - Development Setup

## Prerequisites

- .NET 8.0 SDK
- Docker & Docker Compose
- Git

## Quick Start

### 1. Start Development Environment

```bash
# Make the script executable
chmod +x start-dev.sh

# Start PostgreSQL and Redis
./start-dev.sh
```

### 2. Run the API

```bash
cd src/EcomVideoAI.API
dotnet run
```

The API will be available at:
- **Swagger UI**: http://localhost:5000 (or https://localhost:5001)
- **Health Check**: http://localhost:5000/health

## Development Services

### PostgreSQL Database
- **Host**: localhost:5432
- **Database**: ecomvideoai_dev
- **Username**: postgres
- **Password**: password

### Redis Cache
- **Host**: localhost:6379

### PgAdmin (Database Management)
- **URL**: http://localhost:8080
- **Email**: admin@ecomvideoai.com
- **Password**: admin123

## Database Management

### Entity Framework Migrations

```bash
# Navigate to the API project
cd src/EcomVideoAI.API

# Add a new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Drop database (careful!)
dotnet ef database drop
```

### Manual Database Operations

```bash
# Connect to PostgreSQL container
docker exec -it ecomvideoai_postgres_dev psql -U postgres -d ecomvideoai_dev

# View logs
docker logs ecomvideoai_postgres_dev
```

## Configuration

### Environment Variables

Create `src/EcomVideoAI.API/appsettings.Development.json` for local overrides:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ecomvideoai_dev;Username=postgres;Password=password;Port=5432"
  },
  "Freepik": {
    "ApiKey": "your-freepik-api-key-here"
  }
}
```

### Freepik API Key

1. Get your API key from [Freepik API](https://api.freepik.com/)
2. Add it to `appsettings.Development.json` or set as environment variable:
   ```bash
   export Freepik__ApiKey="your-api-key"
   ```

## Testing the API

### Create Video from Text

```bash
curl -X POST "http://localhost:5000/api/video/create-from-text" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Test Video",
    "description": "A test video",
    "textPrompt": "A beautiful sunset over mountains",
    "resolution": "HD_720p",
    "duration": 5
  }'
```

## Troubleshooting

### Common Issues

1. **Port conflicts**: If ports 5432, 6379, or 8080 are in use, modify `docker-compose.dev.yml`
2. **Docker not running**: Ensure Docker Desktop is running
3. **Database connection failed**: Check if PostgreSQL container is healthy:
   ```bash
   docker ps
   docker logs ecomvideoai_postgres_dev
   ```

### Reset Development Environment

```bash
# Stop and remove containers
docker-compose -f docker-compose.dev.yml down -v

# Start fresh
./start-dev.sh
```

## Project Structure

```
EcomVideoAI.Backend/
├── src/
│   ├── EcomVideoAI.API/              # Web API Layer
│   ├── EcomVideoAI.Application/      # Business Logic
│   ├── EcomVideoAI.Domain/           # Domain Models
│   ├── EcomVideoAI.Infrastructure/   # Data & External Services
│   └── EcomVideoAI.Shared/           # Common Utilities
├── docker-compose.dev.yml            # Development containers
├── start-dev.sh                      # Quick start script
└── README.dev.md                     # This file
```

## Next Steps

1. Set up your Freepik API key
2. Test the video creation endpoint
3. Implement additional use cases
4. Add authentication
5. Set up CI/CD pipeline 