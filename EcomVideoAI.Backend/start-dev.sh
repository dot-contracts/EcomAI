#!/bin/bash

# EcomVideo AI Development Environment Setup Script

echo "🚀 Starting EcomVideo AI Backend..."

# Navigate to the script's directory to ensure relative paths work
cd "$(dirname "$0")"

# Navigate to the API project directory
cd src/EcomVideoAI.API

# Run the .NET API
echo "🚀 Running the .NET API with 'dotnet run' in Development mode..."
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://0.0.0.0:5000"
dotnet run 