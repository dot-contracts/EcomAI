#!/bin/bash

# EcomVideoAI Backend Deployment Script for DigitalOcean
set -e

echo "🚀 EcomVideoAI Backend Deployment to DigitalOcean"
echo "================================================"

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

# Check if doctl is installed
if ! command -v doctl &> /dev/null; then
    print_error "DigitalOcean CLI (doctl) is not installed."
    echo "Please install it from: https://docs.digitalocean.com/reference/doctl/how-to/install/"
    exit 1
fi

# Check if user is authenticated
if ! doctl account get &> /dev/null; then
    print_error "You are not authenticated with DigitalOcean CLI."
    echo "Please run: doctl auth init"
    exit 1
fi

print_success "DigitalOcean CLI is ready!"

# Check if .env file exists for secrets
if [[ ! -f .env ]]; then
    print_warning ".env file not found. Creating template..."
    cat > .env << EOF
# Database
POSTGRES_PASSWORD=your-secure-database-password

# JWT Configuration
JWT_SECRET_KEY=your-super-secret-jwt-key-change-this-in-production

# Freepik API
FREEPIK_API_KEY=your-freepik-api-key

# App Configuration
APP_NAME=ecomvideoai-backend
GITHUB_REPO=your-username/your-repo-name
DOMAIN=your-domain.com
EOF
    print_warning "Please edit .env file with your actual values before continuing."
    exit 1
fi

# Load environment variables
source .env

print_status "Loading configuration..."

# Validate required environment variables
required_vars=("POSTGRES_PASSWORD" "JWT_SECRET_KEY" "FREEPIK_API_KEY" "APP_NAME")
for var in "${required_vars[@]}"; do
    if [[ -z "${!var}" ]]; then
        print_error "Required environment variable $var is not set in .env file"
        exit 1
    fi
done

print_success "Configuration loaded successfully!"

# Menu for deployment options
echo ""
echo "Choose deployment option:"
echo "1) Deploy using App Platform (Recommended)"
echo "2) Deploy using Droplets (Advanced)"
echo "3) Create database only"
echo "4) Test local Docker build"
echo "5) Exit"
echo ""

read -p "Enter your choice (1-5): " choice

case $choice in
    1)
        print_status "Deploying using DigitalOcean App Platform..."
        
        # Update app.yaml with actual values
        sed -i.bak "s/your-username\/your-repo-name/$GITHUB_REPO/g" .do/app.yaml
        sed -i.bak "s/https:\/\/yourapp.com/https:\/\/$DOMAIN/g" .do/app.yaml
        
        print_status "Creating app on DigitalOcean..."
        
        # Create the app
        if doctl apps create .do/app.yaml; then
            print_success "App created successfully!"
            
            # Get app ID
            APP_ID=$(doctl apps list --format ID --no-header | head -n 1)
            
            print_status "Setting up secrets..."
            
            # Create secrets
            doctl apps create-deployment $APP_ID --env JWT_SECRET_KEY="$JWT_SECRET_KEY"
            doctl apps create-deployment $APP_ID --env FREEPIK_API_KEY="$FREEPIK_API_KEY"
            
            print_success "Deployment initiated!"
            print_status "You can monitor the deployment at: https://cloud.digitalocean.com/apps"
            
            # Get app URL
            sleep 10
            APP_URL=$(doctl apps get $APP_ID --format LiveURL --no-header)
            print_success "Your API will be available at: $APP_URL"
            
        else
            print_error "Failed to create app. Please check your configuration."
            exit 1
        fi
        ;;
        
    2)
        print_status "Droplet deployment not implemented in this script."
        print_status "Please refer to the manual deployment guide in DEPLOYMENT.md"
        ;;
        
    3)
        print_status "Creating managed PostgreSQL database..."
        
        if doctl databases create ecomvideoai-db --engine pg --version 15 --size db-s-1vcpu-1gb --region nyc1; then
            print_success "Database created successfully!"
            print_status "Waiting for database to be ready..."
            
            # Wait for database to be ready
            while [[ $(doctl databases get ecomvideoai-db --format Status --no-header) != "online" ]]; do
                print_status "Database is still initializing..."
                sleep 30
            done
            
            print_success "Database is ready!"
            
            # Get database connection details
            doctl databases connection ecomvideoai-db --format URI
            
        else
            print_error "Failed to create database."
            exit 1
        fi
        ;;
        
    4)
        print_status "Testing local Docker build..."
        
        if docker build -t ecomvideoai-backend .; then
            print_success "Docker build successful!"
            print_status "You can test it locally with: docker run -p 5000:5000 ecomvideoai-backend"
        else
            print_error "Docker build failed."
            exit 1
        fi
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
echo "Next steps:"
echo "1. Update your React Native app configuration with the new API URL"
echo "2. Update CORS settings in your backend if needed"
echo "3. Test your API endpoints"
echo "4. Set up monitoring and alerts"
echo ""
echo "Useful commands:"
echo "- Check app status: doctl apps list"
echo "- View logs: doctl apps logs <app-id> --follow"
echo "- Update app: doctl apps update <app-id> .do/app.yaml" 