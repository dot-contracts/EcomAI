# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj files and restore dependencies
COPY src/EcomVideoAI.API/*.csproj ./src/EcomVideoAI.API/
COPY src/EcomVideoAI.Application/*.csproj ./src/EcomVideoAI.Application/
COPY src/EcomVideoAI.Domain/*.csproj ./src/EcomVideoAI.Domain/
COPY src/EcomVideoAI.Infrastructure/*.csproj ./src/EcomVideoAI.Infrastructure/

# Restore dependencies
RUN dotnet restore src/EcomVideoAI.API/EcomVideoAI.API.csproj

# Copy the rest of the source code
COPY . .

# Build the application
RUN dotnet publish src/EcomVideoAI.API/EcomVideoAI.API.csproj -c Release -o out

# Use the official .NET 8 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=build /app/out .

# Create logs directory
RUN mkdir -p logs

# Expose port 5000
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "EcomVideoAI.API.dll"]