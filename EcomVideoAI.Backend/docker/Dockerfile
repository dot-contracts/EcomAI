# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/EcomVideoAI.API/EcomVideoAI.API.csproj", "src/EcomVideoAI.API/"]
COPY ["src/EcomVideoAI.Application/EcomVideoAI.Application.csproj", "src/EcomVideoAI.Application/"]
COPY ["src/EcomVideoAI.Domain/EcomVideoAI.Domain.csproj", "src/EcomVideoAI.Domain/"]
COPY ["src/EcomVideoAI.Infrastructure/EcomVideoAI.Infrastructure.csproj", "src/EcomVideoAI.Infrastructure/"]
COPY ["src/EcomVideoAI.Shared/EcomVideoAI.Shared.csproj", "src/EcomVideoAI.Shared/"]

RUN dotnet restore "src/EcomVideoAI.API/EcomVideoAI.API.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/EcomVideoAI.API"
RUN dotnet build "EcomVideoAI.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "EcomVideoAI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user
RUN adduser --disabled-password --home /app --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "EcomVideoAI.API.dll"] 