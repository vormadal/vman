# Unified Dockerfile for Video Manager - Frontend + Backend
# This Dockerfile builds both the Next.js frontend and .NET backend into a single container

# Stage 1: Build Next.js Frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app/frontend

# Copy frontend package files
COPY video-manager-frontend/package*.json ./

# Install dependencies
RUN npm ci

# Copy frontend source code
COPY video-manager-frontend/ ./

# Build the Next.js application (standalone mode)
RUN npm run build

# Stage 2: Build .NET Backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

# Copy all VideoManager content including tools config
COPY VideoManager/ VideoManager/

# Restore tools and dependencies
WORKDIR /src/VideoManager
RUN dotnet tool restore

# Restore project dependencies
RUN dotnet restore VManBackend/VManBackend.csproj

# Build and publish the backend
WORKDIR /src/VideoManager/VManBackend
RUN mkdir -p bin/Debug
RUN dotnet publish VManBackend.csproj -c Release -o /app/backend --no-restore

# Stage 3: Final Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy backend published files
COPY --from=backend-build /app/backend ./

# Copy Next.js standalone build to wwwroot
COPY --from=frontend-build /app/frontend/.next/standalone ./wwwroot/
COPY --from=frontend-build /app/frontend/.next/static ./wwwroot/.next/static
COPY --from=frontend-build /app/frontend/public ./wwwroot/public

# Expose port (Caprover uses 80 by default)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the backend (which now serves the frontend)
ENTRYPOINT ["dotnet", "VManBackend.dll"]
