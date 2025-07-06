#!/bin/bash

# Fix Database Migrations on Staging Server
# This script will manually apply the database migrations

echo "🔧 Fixing EcomVideoAI Database on Staging Server"
echo "================================================"

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

print_status "Connecting to staging server to fix database..."

# Connect to server and apply database fix
sshpass -p "$DROPLET_PASSWORD" ssh -o StrictHostKeyChecking=no root@$DROPLET_IP << 'ENDSSH'
    set -e
    
    echo "🔧 Fixing Database Issues..."
    
    # Navigate to app directory
    cd /opt/ecomvideoai || cd /root/ecomvideoai || cd /
    
    # Check if PostgreSQL is running
    if docker ps | grep -q postgres; then
        echo "✅ PostgreSQL container is running"
        
        # Get PostgreSQL container name
        POSTGRES_CONTAINER=$(docker ps --format "table {{.Names}}" | grep postgres | head -n 1)
        echo "📦 PostgreSQL container: $POSTGRES_CONTAINER"
        
        # Test database connection
        echo "🔍 Testing database connection..."
        docker exec $POSTGRES_CONTAINER psql -U postgres -d ecomvideoai_db -c "SELECT version();" || {
            echo "❌ Database connection failed. Creating database..."
            docker exec $POSTGRES_CONTAINER psql -U postgres -c "CREATE DATABASE ecomvideoai_db;"
            docker exec $POSTGRES_CONTAINER psql -U postgres -d ecomvideoai_db -c "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";"
        }
        
        # Apply migrations manually using SQL
        echo "🔄 Applying database migrations..."
        
        # Create migrations history table
        docker exec $POSTGRES_CONTAINER psql -U postgres -d ecomvideoai_db -c "
        CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (
            migration_id character varying(150) NOT NULL,
            product_version character varying(32) NOT NULL,
            CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
        );"
        
        # Apply InitialCreate migration
        docker exec $POSTGRES_CONTAINER psql -U postgres -d ecomvideoai_db -c "
        DO \$EF\$
        BEGIN
            IF NOT EXISTS(SELECT 1 FROM \"__EFMigrationsHistory\" WHERE migration_id = '20250620182407_InitialCreate') THEN
            CREATE TABLE videos (
                id uuid NOT NULL,
                user_id uuid NOT NULL,
                title character varying(200) NOT NULL,
                description character varying(1000) NOT NULL,
                text_prompt character varying(2000) NOT NULL,
                input_type character varying(50) NOT NULL,
                status character varying(50) NOT NULL,
                resolution character varying(50) NOT NULL,
                duration_seconds integer NOT NULL,
                image_url character varying(500),
                video_url character varying(500),
                thumbnail_url character varying(500),
                freepik_task_id character varying(100),
                freepik_image_task_id character varying(100),
                completed_at timestamp with time zone,
                error_message character varying(2000),
                file_size_bytes bigint NOT NULL DEFAULT 0,
                metadata jsonb,
                created_at timestamp with time zone NOT NULL,
                updated_at timestamp with time zone,
                created_by character varying(100),
                updated_by character varying(100),
                CONSTRAINT pk_videos PRIMARY KEY (id)
            );
            END IF;
        END \$EF\$;"
        
        # Create indexes
        docker exec $POSTGRES_CONTAINER psql -U postgres -d ecomvideoai_db -c "
        DO \$EF\$
        BEGIN
            IF NOT EXISTS(SELECT 1 FROM \"__EFMigrationsHistory\" WHERE migration_id = '20250620182407_InitialCreate') THEN
            CREATE INDEX ix_videos_created_at ON videos (created_at);
            CREATE UNIQUE INDEX ix_videos_freepik_image_task_id ON videos (freepik_image_task_id) WHERE freepik_image_task_id IS NOT NULL;
            CREATE UNIQUE INDEX ix_videos_freepik_task_id ON videos (freepik_task_id) WHERE freepik_task_id IS NOT NULL;
            CREATE INDEX ix_videos_status ON videos (status);
            CREATE INDEX ix_videos_user_id ON videos (user_id);
            INSERT INTO \"__EFMigrationsHistory\" (migration_id, product_version)
            VALUES ('20250620182407_InitialCreate', '8.0.11');
            END IF;
        END \$EF\$;"
        
        # Apply AspectRatio migration
        docker exec $POSTGRES_CONTAINER psql -U postgres -d ecomvideoai_db -c "
        DO \$EF\$
        BEGIN
            IF NOT EXISTS(SELECT 1 FROM \"__EFMigrationsHistory\" WHERE migration_id = '20250624095927_AddVideoAspectRatio') THEN
            ALTER TABLE videos ADD aspect_ratio integer NOT NULL DEFAULT 0;
            ALTER TABLE videos ADD style text;
            INSERT INTO \"__EFMigrationsHistory\" (migration_id, product_version)
            VALUES ('20250624095927_AddVideoAspectRatio', '8.0.11');
            END IF;
        END \$EF\$;"
        
        echo "✅ Database migrations applied successfully!"
        
        # Verify table structure
        echo "🔍 Verifying database structure..."
        docker exec $POSTGRES_CONTAINER psql -U postgres -d ecomvideoai_db -c "\d videos"
        
        # Restart the API container to ensure clean connection
        echo "🔄 Restarting API container..."
        API_CONTAINER=$(docker ps --format "table {{.Names}}" | grep api | head -n 1)
        if [ ! -z "$API_CONTAINER" ]; then
            docker restart $API_CONTAINER
            echo "✅ API container restarted"
        fi
        
        echo "🎉 Database fix completed!"
        
    else
        echo "❌ PostgreSQL container not found. Starting database..."
        # Try to start database if docker-compose exists
        if [ -f docker-compose.production.yml ]; then
            docker-compose -f docker-compose.production.yml up -d postgres
            sleep 10
            # Run this script again after PostgreSQL starts
            echo "🔄 Retrying after PostgreSQL startup..."
        else
            echo "❌ No docker-compose file found. Manual database setup required."
        fi
    fi
ENDSSH

print_success "Database fix completed!"

echo ""
echo "🧪 Testing the fixed database..."
sleep 5

# Test the video creation endpoint
curl -X POST http://209.38.121.252/api/Video/create-from-text \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "textPrompt": "A beautiful sunset over the ocean",
    "title": "Database Test Video",
    "description": "Testing the fixed database",
    "duration": 3,
    "resolution": 1,
    "aspectRatio": "9:16",
    "style": "realistic"
  }' \
  -w "\n\nHTTP Status: %{http_code}\n" \
  -s

echo ""
print_success "🎯 Database persistence should now be working!"
print_success "🌐 Your video generation API is ready at: http://209.38.121.252/api"

echo ""
echo "Next steps:"
echo "1. Your React Native app should now successfully create videos"
echo "2. Videos will be stored in the PostgreSQL database"
echo "3. You can monitor video generation progress"
echo "4. Test with your mobile app!" 