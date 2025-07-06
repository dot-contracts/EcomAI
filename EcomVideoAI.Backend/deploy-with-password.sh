#!/bin/bash

# EcomVideoAI Backend Deployment Script for DigitalOcean Droplet with Password Auth
set -e

echo "🚀 EcomVideoAI Backend Deployment to DigitalOcean Droplet"
echo "=========================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Configuration
DROPLET_IP="209.38.121.252"
DROPLET_PASSWORD="Promax@9000"
APP_NAME="ecomvideoai-backend"
DOCKER_IMAGE="ecomvideoai-api"

print_status "Deploying to Droplet: $DROPLET_IP"

# Check if sshpass is installed
if ! command -v sshpass &> /dev/null; then
    print_error "sshpass is not installed. Please install it first:"
    echo "sudo apt-get install sshpass"
    exit 1
fi

print_status "Creating deployment package..."

# Create a tar file with the necessary files
tar -czf ../ecomvideoai-deployment.tar.gz \
    --exclude='.git' \
    --exclude='bin' \
    --exclude='obj' \
    --exclude='logs' \
    --exclude='.env' \
    .

print_status "Uploading to Droplet..."

# Upload the deployment package
sshpass -p "$DROPLET_PASSWORD" scp -o StrictHostKeyChecking=no ../ecomvideoai-deployment.tar.gz root@$DROPLET_IP:/tmp/

print_status "Deploying on Droplet..."

# Deploy on the droplet
sshpass -p "$DROPLET_PASSWORD" ssh -o StrictHostKeyChecking=no root@$DROPLET_IP << 'ENDSSH'
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
    
    # Extract the deployment package
    tar -xzf /tmp/ecomvideoai-deployment.tar.gz
    
    # Stop existing containers
    docker-compose -f docker-compose.production.yml down || true
    
    # Build and start the application
    echo "🚀 Building and starting the application..."
    docker-compose -f docker-compose.production.yml up --build -d
    
    echo "✅ Deployment completed!"
    echo "🌐 Your API is available at: http://209.38.121.252"
    echo "🔍 Health check: http://209.38.121.252/health"
    
    # Show running containers
    docker ps
    
    # Show logs
    echo "📋 Recent logs:"
    docker-compose -f docker-compose.production.yml logs --tail=20
ENDSSH

print_success "Deployment completed!"
print_success "🌐 Your API is available at: http://$DROPLET_IP"
print_success "🔍 Health check: http://$DROPLET_IP/health"

# Clean up local tar file
rm -f ../ecomvideoai-deployment.tar.gz

print_success "Deployment script completed!"
echo ""
echo "🎉 Next steps:"
echo "1. Test your API: curl http://$DROPLET_IP/health"
echo "2. Update your React Native app configuration:"
echo "   API_URL: http://$DROPLET_IP/api"
echo "3. Set up SSL certificate (optional but recommended)"
echo "4. Configure domain name (optional)"
echo ""
echo "📊 Useful commands for your Droplet:"
echo "- Check logs: sshpass -p '$DROPLET_PASSWORD' ssh root@$DROPLET_IP 'docker logs ecomvideoai-api-prod'"
echo "- Restart app: sshpass -p '$DROPLET_PASSWORD' ssh root@$DROPLET_IP 'docker restart ecomvideoai-api-prod'"
echo "- Monitor: sshpass -p '$DROPLET_PASSWORD' ssh root@$DROPLET_IP 'docker stats'" 