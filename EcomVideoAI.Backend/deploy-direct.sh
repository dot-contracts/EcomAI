#!/bin/bash

# Direct deployment script that handles password-protected droplet
set -e

echo "🚀 EcomVideoAI Direct Deployment to DigitalOcean Droplet"
echo "========================================================"

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

# Configuration
DROPLET_IP="209.38.121.252"
OLD_PASSWORD="c4f9495dd734a243dd9f56cc57"
NEW_PASSWORD="EcomVideoAI2024!"
APP_NAME="ecomvideoai-backend"
DOCKER_IMAGE="ecomvideoai-api"

print_status "Step 1: Building Docker image locally..."

# Build Docker image
if docker build -t $DOCKER_IMAGE .; then
    print_success "Docker build successful!"
else
    print_error "Docker build failed."
    exit 1
fi

print_status "Step 2: Saving Docker image to file..."
docker save $DOCKER_IMAGE > ../ecomvideoai-image.tar

print_status "Step 3: Creating deployment package..."
tar -czf ../ecomvideoai-deploy.tar.gz \
    docker-compose.production.yml \
    ../ecomvideoai-image.tar

print_status "Step 4: Attempting to deploy to droplet..."

# Try to handle password change and deployment
expect << EOF
set timeout 60

# First, try to connect and change password
spawn sshpass -p "$OLD_PASSWORD" ssh -o StrictHostKeyChecking=no root@$DROPLET_IP

expect {
    "Current password:" {
        send "$OLD_PASSWORD\r"
        expect "New password:"
        send "$NEW_PASSWORD\r"
        expect "Retype new password:"
        send "$NEW_PASSWORD\r"
        expect {
            "password updated successfully" {
                send "exit\r"
                expect eof
            }
            "passwd:" {
                send "exit\r"
                expect eof
            }
        }
    }
    "root@" {
        send "exit\r"
        expect eof
    }
    timeout {
        puts "Timeout during password change"
    }
}
EOF

print_status "Step 5: Uploading deployment package..."

# Try with both old and new passwords
sshpass -p "$NEW_PASSWORD" scp -o StrictHostKeyChecking=no ../ecomvideoai-deploy.tar.gz root@$DROPLET_IP:/tmp/ || \
sshpass -p "$OLD_PASSWORD" scp -o StrictHostKeyChecking=no ../ecomvideoai-deploy.tar.gz root@$DROPLET_IP:/tmp/

print_status "Step 6: Deploying on droplet..."

# Deploy with the password that works
sshpass -p "$NEW_PASSWORD" ssh -o StrictHostKeyChecking=no root@$DROPLET_IP << 'ENDSSH' || \
sshpass -p "$OLD_PASSWORD" ssh -o StrictHostKeyChecking=no root@$DROPLET_IP << 'ENDSSH'
    set -e
    
    echo "🔧 Setting up deployment environment..."
    
    # Install Docker if not present
    if ! command -v docker &> /dev/null; then
        echo "Installing Docker..."
        curl -fsSL https://get.docker.com -o get-docker.sh
        sh get-docker.sh
        systemctl start docker
        systemctl enable docker
    fi
    
    # Install Docker Compose if not present
    if ! command -v docker-compose &> /dev/null; then
        echo "Installing Docker Compose..."
        curl -L "https://github.com/docker/compose/releases/download/v2.21.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
        chmod +x /usr/local/bin/docker-compose
    fi
    
    # Create app directory
    mkdir -p /opt/ecomvideoai
    cd /opt/ecomvideoai
    
    # Extract deployment package
    tar -xzf /tmp/ecomvideoai-deploy.tar.gz
    
    # Load Docker image
    docker load < ecomvideoai-image.tar
    
    # Stop existing containers
    docker-compose -f docker-compose.production.yml down || true
    
    # Start the application
    echo "🚀 Starting the application..."
    docker-compose -f docker-compose.production.yml up -d
    
    echo "✅ Deployment completed!"
    echo "🌐 Your API is available at: http://209.38.121.252"
    echo "🔍 Health check: http://209.38.121.252/health"
    
    # Show running containers
    docker ps
    
    # Show logs
    echo "📋 Recent logs:"
    docker-compose -f docker-compose.production.yml logs --tail=10
ENDSSH

# Clean up local files
rm -f ../ecomvideoai-image.tar ../ecomvideoai-deploy.tar.gz

print_success "Deployment completed!"
print_success "🌐 Your API is available at: http://$DROPLET_IP"
print_success "🔍 Health check: http://$DROPLET_IP/health"

echo ""
echo "🎉 Next steps:"
echo "1. Test your API: curl http://$DROPLET_IP/health"
echo "2. Update your React Native app configuration:"
echo "   API_URL: http://$DROPLET_IP/api"
echo "3. Test video generation endpoint" 