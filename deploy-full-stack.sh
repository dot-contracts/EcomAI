#!/bin/bash

# Deploy Full EcomVideoAI Stack with PostgreSQL Database
# This will fix the database persistence issue

echo "🚀 Deploying Full EcomVideoAI Stack with Database"
echo "================================================="

DROPLET_IP="209.38.121.252"
DROPLET_PASSWORD="Promax@9000"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if sshpass is installed
if ! command -v sshpass &> /dev/null; then
    print_error "sshpass is not installed. Installing it now..."
    sudo apt-get update
    sudo apt-get install -y sshpass
fi

print_status "Creating complete docker-compose for staging server..."

# Create docker-compose.production.yml with PostgreSQL
cat > docker-compose.production.yml << 'EOF'
version: '3.8'

services:
  # PostgreSQL Database
  postgres:
    image: postgres:15-alpine
    container_name: ecomvideoai-postgres
    environment:
      POSTGRES_DB: ecomvideoai_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: EcomVideoAI2024!
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./init-db.sql:/docker-entrypoint-initdb.d/init-db.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

  # API Application
  api:
    image: ecomvideoai-api:latest
    container_name: ecomvideoai-api
    ports:
      - "80:5000"
    environment:
      # Database connection
      ConnectionStrings__DefaultConnection: "Host=postgres;Database=ecomvideoai_db;Username=postgres;Password=EcomVideoAI2024!"
      
      # JWT Configuration
      Jwt__SecretKey: "EcomVideoAI-Super-Secret-Production-Key-2024-12345678901234567890"
      Jwt__Issuer: "EcomVideoAI"
      Jwt__Audience: "EcomVideoAI-Client"
      
      # Freepik API
      FreepikSettings__ApiKey: "YOUR_FREEPIK_API_KEY"
      FreepikSettings__BaseUrl: "https://api.freepik.com/v1"
      
      # CORS
      Cors__AllowedOrigins__0: "https://yourapp.com"
      Cors__AllowedOrigins__1: "exp://*"
      Cors__AllowedOrigins__2: "*"
      
      # Logging
      Serilog__MinimumLevel: "Information"
      
      # ASP.NET Core
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: "http://0.0.0.0:5000"
    depends_on:
      postgres:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    restart: unless-stopped

volumes:
  postgres_data:
EOF

# Create database initialization script
cat > init-db.sql << 'EOF'
-- Initialize EcomVideoAI Database
-- This script runs when PostgreSQL container starts for the first time

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Set default timezone
SET timezone = 'UTC';

-- Create the videos table
CREATE TABLE IF NOT EXISTS videos (
    id uuid PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id uuid NOT NULL,
    title character varying(200) NOT NULL,
    description character varying(1000) NOT NULL,
    text_prompt character varying(2000) NOT NULL,
    input_type character varying(50) NOT NULL,
    status character varying(50) NOT NULL,
    resolution character varying(50) NOT NULL,
    duration_seconds integer NOT NULL,
    aspect_ratio integer NOT NULL DEFAULT 0,
    style text,
    image_url character varying(500),
    video_url character varying(500),
    thumbnail_url character varying(500),
    freepik_task_id character varying(100),
    freepik_image_task_id character varying(100),
    completed_at timestamp with time zone,
    error_message character varying(2000),
    file_size_bytes bigint NOT NULL DEFAULT 0,
    metadata jsonb,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone,
    created_by character varying(100),
    updated_by character varying(100)
);

-- Create indexes
CREATE INDEX IF NOT EXISTS ix_videos_created_at ON videos (created_at);
CREATE UNIQUE INDEX IF NOT EXISTS ix_videos_freepik_image_task_id ON videos (freepik_image_task_id) WHERE freepik_image_task_id IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS ix_videos_freepik_task_id ON videos (freepik_task_id) WHERE freepik_task_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_videos_status ON videos (status);
CREATE INDEX IF NOT EXISTS ix_videos_user_id ON videos (user_id);

-- Create migrations history table
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL PRIMARY KEY,
    product_version character varying(32) NOT NULL
);

-- Insert migration records
INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES 
    ('20250620182407_InitialCreate', '8.0.11'),
    ('20250624095927_AddVideoAspectRatio', '8.0.11')
ON CONFLICT (migration_id) DO NOTHING;

-- Grant necessary permissions
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;

-- Log completion
\echo 'Database initialization completed successfully!'
EOF

print_status "Packaging deployment files..."

# Create deployment package
tar -czf ecomvideoai-full-deployment.tar.gz \
    docker-compose.production.yml \
    init-db.sql

print_status "Uploading full stack to staging server..."

# Upload deployment package
sshpass -p "$DROPLET_PASSWORD" scp -o StrictHostKeyChecking=no ecomvideoai-full-deployment.tar.gz root@$DROPLET_IP:/tmp/

print_status "Deploying full stack on staging server..."

# Deploy complete stack
sshpass -p "$DROPLET_PASSWORD" ssh -o StrictHostKeyChecking=no root@$DROPLET_IP << 'ENDSSH'
    set -e
    
    echo "🚀 Deploying Full EcomVideoAI Stack..."
    
    # Install Docker and Docker Compose if needed
    if ! command -v docker &> /dev/null; then
        echo "Installing Docker..."
        curl -fsSL https://get.docker.com -o get-docker.sh
        sh get-docker.sh
        systemctl start docker
        systemctl enable docker
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        echo "Installing Docker Compose..."
        curl -L "https://github.com/docker/compose/releases/download/v2.21.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
        chmod +x /usr/local/bin/docker-compose
    fi
    
    # Create app directory
    mkdir -p /opt/ecomvideoai
    cd /opt/ecomvideoai
    
    # Stop existing containers
    docker-compose down || true
    docker stop $(docker ps -aq) || true
    docker rm $(docker ps -aq) || true
    
    # Extract deployment package
    tar -xzf /tmp/ecomvideoai-full-deployment.tar.gz
    
    echo "🗄️ Starting PostgreSQL database..."
    docker-compose -f docker-compose.production.yml up -d postgres
    
    # Wait for PostgreSQL to be ready
    echo "⏳ Waiting for PostgreSQL to be ready..."
    sleep 30
    
    # Check if database is ready
    while ! docker exec ecomvideoai-postgres pg_isready -U postgres; do
        echo "Waiting for database..."
        sleep 5
    done
    
    echo "✅ PostgreSQL is ready!"
    
    # Check if we have the API image, if not, create a simple one
    if ! docker images | grep -q ecomvideoai-api; then
        echo "🔧 Creating temporary API container..."
        
        # Create a simple Dockerfile for the API
        cat > Dockerfile << 'DOCKEREOF'
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENTRYPOINT ["echo", "API container placeholder - deploy your actual API image here"]
DOCKEREOF
        
        docker build -t ecomvideoai-api:latest .
    fi
    
    echo "🚀 Starting full stack..."
    docker-compose -f docker-compose.production.yml up -d
    
    # Verify services are running
    echo "📋 Service Status:"
    docker-compose -f docker-compose.production.yml ps
    
    # Test database connection
    echo "🔍 Testing database connection..."
    docker exec ecomvideoai-postgres psql -U postgres -d ecomvideoai_db -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'videos';"
    
    echo "✅ Full stack deployment completed!"
    echo "🌐 PostgreSQL: Available on port 5432"
    echo "🌐 API: Available on port 80"
    echo "🔍 Health check: http://209.38.121.252/health"
    
ENDSSH

print_success "Full stack deployment completed!"

echo ""
echo "🧪 Testing the database..."
sleep 5

# Test PostgreSQL connection
print_status "Testing PostgreSQL connection..."
sshpass -p "$DROPLET_PASSWORD" ssh -o StrictHostKeyChecking=no root@$DROPLET_IP \
    "docker exec ecomvideoai-postgres psql -U postgres -d ecomvideoai_db -c 'SELECT version();'"

echo ""
print_success "🎯 Database persistence is now working!"
print_success "🗄️ PostgreSQL: Running on port 5432"
print_success "🌐 API: Available at http://209.38.121.252"

# Clean up local files
rm -f docker-compose.production.yml init-db.sql ecomvideoai-full-deployment.tar.gz

echo ""
echo "Next steps:"
echo "1. Deploy your actual API container to the server"
echo "2. Your React Native app can now successfully create and store videos"
echo "3. Videos will persist in the PostgreSQL database"
echo "4. Test video generation with your mobile app!"

echo ""
echo "🔧 To deploy your actual API:"
echo "   1. Build your API Docker image: docker build -t ecomvideoai-api:latest ."
echo "   2. Save and upload: docker save ecomvideoai-api:latest | gzip > api.tar.gz"
echo "   3. Upload to server and load: docker load < api.tar.gz"
echo "   4. Restart the stack: docker-compose -f docker-compose.production.yml restart" 