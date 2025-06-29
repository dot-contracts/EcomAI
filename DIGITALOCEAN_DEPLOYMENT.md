# 🚀 DigitalOcean Deployment Guide

Complete guide to deploy EcomVideoAI backend to DigitalOcean with PostgreSQL database.

## 📋 Prerequisites

1. **DigitalOcean Account** - [Sign up here](https://www.digitalocean.com/)
2. **GitHub Repository** - Your code needs to be in a GitHub repo
3. **Domain Name** (optional but recommended)
4. **Freepik API Key** - For video generation

## 🛠️ Setup Instructions

### Step 1: Install DigitalOcean CLI

```bash
# Ubuntu/Debian
curl -sL https://github.com/digitalocean/doctl/releases/download/v1.98.0/doctl-1.98.0-linux-amd64.tar.gz | tar -xzv
sudo mv doctl /usr/local/bin

# macOS
brew install doctl

# Windows
# Download from: https://github.com/digitalocean/doctl/releases
```

### Step 2: Authenticate with DigitalOcean

1. Get your API token from [DigitalOcean API page](https://cloud.digitalocean.com/account/api/tokens)
2. Run authentication:
```bash
doctl auth init
# Enter your API token when prompted
```

### Step 3: Prepare Your Repository

1. **Push your code to GitHub** (if not already done):
```bash
git add .
git commit -m "Prepare for DigitalOcean deployment"
git push origin main
```

2. **Update repository URL** in `.do/app.yaml`:
```yaml
github:
  repo: your-username/AI-Video-Gen  # Update this line
  branch: main
```

### Step 4: Configure Environment Variables

Create `.env` file in the backend directory:

```bash
cd EcomVideoAI.Backend
cp .env.example .env  # If you have a template
# OR create new .env file:
```

```env
# Database
POSTGRES_PASSWORD=your-super-secure-database-password-123

# JWT Configuration (Generate a strong secret)
JWT_SECRET_KEY=your-super-secret-jwt-key-minimum-32-characters-long

# Freepik API (Get from Freepik developer portal)
FREEPIK_API_KEY=your-freepik-api-key

# App Configuration
APP_NAME=ecomvideoai-backend
GITHUB_REPO=your-username/AI-Video-Gen
DOMAIN=api.yourdomain.com
```

**🔐 Security Note**: Never commit `.env` file to Git! Add it to `.gitignore`.

## 🚀 Deployment Options

### Option A: Automated Deployment (Recommended)

Run the deployment script:

```bash
cd EcomVideoAI.Backend
./deploy.sh
```

Choose option 1 for App Platform deployment.

### Option B: Manual Deployment

#### 1. Create PostgreSQL Database

```bash
# Create managed database
doctl databases create ecomvideoai-db \
  --engine pg \
  --version 15 \
  --size db-s-1vcpu-1gb \
  --region nyc1

# Wait for database to be ready
doctl databases get ecomvideoai-db
```

#### 2. Create App Platform Application

```bash
# Deploy the app
doctl apps create .do/app.yaml

# Get app ID
APP_ID=$(doctl apps list --format ID --no-header | head -n 1)
echo "App ID: $APP_ID"
```

#### 3. Set Environment Variables

```bash
# Set secrets via CLI
doctl apps update $APP_ID --env JWT_SECRET_KEY="your-jwt-secret"
doctl apps update $APP_ID --env FREEPIK_API_KEY="your-freepik-key"
```

#### 4. Monitor Deployment

```bash
# Check deployment status
doctl apps get $APP_ID

# View logs
doctl apps logs $APP_ID --follow
```

## 🔧 Configuration Updates

### Update React Native App Configuration

Once deployed, update your React Native app to use the production API:

1. **Get your app URL**:
```bash
doctl apps get $APP_ID --format LiveURL --no-header
```

2. **Update `api.config.ts`**:
```typescript
// Production environment
return {
  baseURL: 'https://your-app-name-xyz123.ondigitalocean.app/api',
  timeout: 30000,
};
```

3. **Or use environment variable**:
```bash
# For production builds
export REACT_APP_API_URL=https://your-app-name-xyz123.ondigitalocean.app/api
expo build:android
```

### Update CORS Settings

Update your backend's CORS configuration in `Program.cs`:

```csharp
var allowedOrigins = new[] { 
  "https://yourdomain.com",
  "exp://your-expo-app-url",
  "https://your-app-name-xyz123.ondigitalocean.app"
};
```

## 💰 Cost Estimation

### DigitalOcean App Platform Pricing (as of 2024):

- **Basic App**: $5/month (512MB RAM, 1 vCPU)
- **Professional App**: $12/month (1GB RAM, 1 vCPU)
- **PostgreSQL Database**: $15/month (Basic - 1GB RAM, 1 vCPU, 10GB storage)

**Total monthly cost**: ~$20-27/month for a production setup.

### Scaling Options:

- Start with Basic app + Basic database (~$20/month)
- Scale up as your user base grows
- Add CDN, monitoring, and backups as needed

## 🔍 Testing Your Deployment

### 1. Health Check

```bash
curl https://your-app-url.ondigitalocean.app/health
```

### 2. API Test

```bash
curl -X POST https://your-app-url.ondigitalocean.app/api/Video/create-from-text \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "textPrompt": "Test video",
    "title": "Production Test",
    "description": "Testing production API",
    "duration": 3,
    "resolution": 1,
    "style": "realistic"
  }'
```

### 3. Database Connection

```bash
# Get database connection details
doctl databases connection ecomvideoai-db --format URI

# Test connection (optional)
psql "postgresql://username:password@host:port/database?sslmode=require"
```

## 🔧 Troubleshooting

### Common Issues:

#### 1. Build Failures
```bash
# Check build logs
doctl apps logs $APP_ID --type build

# Common fixes:
# - Ensure .NET 8 SDK is specified in Dockerfile
# - Check file paths in build commands
# - Verify all NuGet packages restore correctly
```

#### 2. Database Connection Issues
```bash
# Check database status
doctl databases get ecomvideoai-db

# Verify connection string format:
# postgresql://username:password@host:port/database?sslmode=require
```

#### 3. Environment Variables
```bash
# List current environment variables
doctl apps get $APP_ID --format Spec.Services[0].Envs

# Update environment variable
doctl apps update $APP_ID --env KEY="new-value"
```

#### 4. CORS Issues
- Update allowed origins in your backend
- Ensure HTTPS is used for production
- Check browser console for CORS errors

### Useful Commands:

```bash
# App management
doctl apps list                    # List all apps
doctl apps get $APP_ID            # Get app details
doctl apps delete $APP_ID         # Delete app

# Database management
doctl databases list              # List databases
doctl databases get DB_ID         # Get database details
doctl databases resize DB_ID --size db-s-2vcpu-2gb  # Scale database

# Logs and monitoring
doctl apps logs $APP_ID --follow  # Follow logs
doctl apps logs $APP_ID --type build  # Build logs only
```

## 🔐 Security Best Practices

1. **Use strong passwords** for database and JWT secrets
2. **Enable SSL/TLS** (automatic with App Platform)
3. **Restrict CORS origins** to your actual domains
4. **Use environment variables** for all secrets
5. **Enable database backups** (available in DigitalOcean)
6. **Monitor logs** for suspicious activity
7. **Keep dependencies updated**

## 📊 Monitoring and Maintenance

### Set up monitoring:

1. **App Platform Metrics** - Built-in monitoring in DO dashboard
2. **Database Monitoring** - Query performance and resource usage
3. **Log Aggregation** - Consider external services like LogDNA
4. **Uptime Monitoring** - Use services like UptimeRobot

### Regular maintenance:

- **Weekly**: Check logs for errors
- **Monthly**: Review resource usage and costs
- **Quarterly**: Update dependencies and security patches

## 🎉 Success!

Once deployed, your API will be available at:
`https://your-app-name-xyz123.ondigitalocean.app`

Your React Native app can now connect to this production API, and you'll have a fully scalable backend infrastructure! 🚀

## 📞 Support

- **DigitalOcean Docs**: https://docs.digitalocean.com/
- **Community**: https://www.digitalocean.com/community/
- **Support**: Available through DigitalOcean dashboard

---

**Next Steps**: Update your mobile app configuration and start building amazing AI-powered videos! 🎬✨ 