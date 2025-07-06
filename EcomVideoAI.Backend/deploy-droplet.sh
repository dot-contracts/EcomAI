#!/bin/bash

# EcomVideoAI Backend Deployment Script for DigitalOcean Droplet
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
APP_NAME="ecomvideoai-backend"
DOCKER_IMAGE="ecomvideoai-api"

print_status "Deploying to Droplet: $DROPLET_IP"

# Menu for deployment options
echo ""
echo "Choose deployment option:"
echo "1) Deploy to Droplet via SSH (Recommended)"
echo "2) Build Docker image locally and transfer"
echo "3) Test local Docker build"
echo "4) Setup Droplet environment"
echo "5) Exit"
echo ""

read -p "Enter your choice (1-5): " choice

case $choice in
    1)
        print_status "Deploying to Droplet via SSH..."
        
        # Check if SSH key exists
        if [[ ! -f ~/.ssh/id_rsa ]]; then
            print_warning "SSH key not found. Please ensure you can SSH to the droplet."
            print_status "You can test with: ssh root@$DROPLET_IP"
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
        scp ../ecomvideoai-deployment.tar.gz root@$DROPLET_IP:/tmp/
        
        print_status "Deploying on Droplet..."
        
        # Deploy on the droplet
        ssh root@$DROPLET_IP << 'ENDSSH'
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
ENDSSH
        
        print_success "Deployment completed!"
        print_success "🌐 Your API is available at: http://$DROPLET_IP"
        print_success "🔍 Health check: http://$DROPLET_IP/health"
        
        # Clean up local tar file
        rm -f ../ecomvideoai-deployment.tar.gz
        ;;
        
    2)
        print_status "Building Docker image locally..."
        
        if docker build -t $DOCKER_IMAGE .; then
            print_success "Docker build successful!"
            
            # Save image to tar file
            docker save $DOCKER_IMAGE > ../ecomvideoai-image.tar
            
            print_status "Uploading image to Droplet..."
            scp ../ecomvideoai-image.tar root@$DROPLET_IP:/tmp/
            
            print_status "Loading and running image on Droplet..."
            ssh root@$DROPLET_IP << ENDSSH
                docker load < /tmp/ecomvideoai-image.tar
                docker stop ecomvideoai-api-prod || true
                docker rm ecomvideoai-api-prod || true
                docker run -d --name ecomvideoai-api-prod -p 80:5000 $DOCKER_IMAGE
                echo "✅ Deployment completed!"
                docker ps
ENDSSH
            
            # Clean up
            rm -f ../ecomvideoai-image.tar
            print_success "Deployment completed!"
        else
            print_error "Docker build failed."
            exit 1
        fi
        ;;
        
    3)
        print_status "Testing local Docker build..."
        
        if docker build -t $DOCKER_IMAGE .; then
            print_success "Docker build successful!"
            print_status "You can test it locally with:"
            echo "docker run -p 5000:5000 $DOCKER_IMAGE"
        else
            print_error "Docker build failed."
            exit 1
        fi
        ;;
        
    4)
        print_status "Setting up Droplet environment..."
        
        ssh root@$DROPLET_IP << 'ENDSSH'
            echo "🔧 Setting up DigitalOcean Droplet for EcomVideoAI..."
            
            # Update system
            apt update && apt upgrade -y
            
            # Install essential packages
            apt install -y curl wget git htop nano ufw
            
            # Install Docker
            if ! command -v docker &> /dev/null; then
                echo "Installing Docker..."
                curl -fsSL https://get.docker.com -o get-docker.sh
                sh get-docker.sh
                systemctl start docker
                systemctl enable docker
            fi
            
            # Install Docker Compose
            if ! command -v docker-compose &> /dev/null; then
                echo "Installing Docker Compose..."
                curl -L "https://github.com/docker/compose/releases/download/v2.21.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
                chmod +x /usr/local/bin/docker-compose
            fi
            
            # Setup firewall
            ufw allow ssh
            ufw allow 80
            ufw allow 443
            ufw --force enable
            
            # Create app directory
            mkdir -p /opt/ecomvideoai
            
            echo "✅ Droplet setup completed!"
            echo "Docker version: $(docker --version)"
            echo "Docker Compose version: $(docker-compose --version)"
ENDSSH
        
        print_success "Droplet setup completed!"
        ;;
        
    5)
        print_status "Exiting..."
        exit 0
        ;;
        
    *)
        print_error "Invalid choice. Please run the script again."
        exit 1
        ;;
esac

print_success "Deployment script completed!"
echo ""
echo "🎉 Next steps:"
echo "1. Test your API: curl http://$DROPLET_IP/health"
echo "2. Update your React Native app configuration:"
echo "   API_URL: http://$DROPLET_IP/api"
echo "3. Set up SSL certificate (optional but recommended)"
echo "4. Configure domain name (optional)"
echo ""
echo "📊 Useful commands on your Droplet:"
echo "- Check logs: ssh root@$DROPLET_IP 'docker logs ecomvideoai-api-prod'"
echo "- Restart app: ssh root@$DROPLET_IP 'docker restart ecomvideoai-api-prod'"
echo "- Monitor: ssh root@$DROPLET_IP 'docker stats'" 