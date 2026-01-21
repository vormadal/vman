# Video Management API - .NET 10 MVC Project Setup

> **Note**: This document contains the original design specification. The actual implementation uses:
> - **Project Name**: `VManBackend` (not `VideoManager.API`)
> - **Orchestration**: .NET Aspire 13.1.0 for service management
> - **OpenAPI**: Built-in .NET 10 OpenAPI support (not Swashbuckle)
> - See `ASPIRE_README.md` for current running instructions

## Project Overview
A Video Management API built with .NET 10 MVC using Vertical Slice Architecture, PostgreSQL database, and Bearer token authentication for managing videos from multiple providers (OneDrive, Immich) with thumbnail and GIF generation capabilities.

## Architecture: Vertical Slice Architecture
Each feature is organized as a self-contained vertical slice containing all layers (API, Business Logic, Data Access) within a single feature folder.

## Technology Stack
- **.NET 10 MVC**
- **PostgreSQL** - Database
- **Entity Framework Core** - ORM
- **JWT Bearer Authentication** - Security
- **FFmpeg** - Video processing (thumbnails & GIF generation)
- **Microsoft Graph SDK** - OneDrive integration
- **Immich SDK/API** - Immich integration

## Project Structure

```
VideoManager.API/
├── Features/
│   ├── Authentication/
│   │   ├── Login.cs
│   │   ├── Register.cs
│   │   ├── RefreshToken.cs
│   │   └── AuthenticationController.cs
│   ├── Videos/
│   │   ├── GetVideos.cs
│   │   ├── GetVideoById.cs
│   │   ├── ImportFromProvider.cs
│   │   ├── DeleteVideo.cs
│   │   └── VideosController.cs
│   ├── Thumbnails/
│   │   ├── GenerateThumbnail.cs
│   │   ├── GetThumbnail.cs
│   │   └── ThumbnailsController.cs
│   └── Previews/
│       ├── GenerateGif.cs
│       ├── GetGifPreview.cs
│       └── PreviewsController.cs
├── Infrastructure/
│   ├── Database/
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/
│   ├── VideoProviders/
│   │   ├── IVideoProvider.cs
│   │   ├── OneDriveProvider.cs
│   │   └── ImmichProvider.cs
│   ├── VideoProcessing/
│   │   ├── IVideoProcessor.cs
│   │   ├── FFmpegVideoProcessor.cs
│   │   ├── ThumbnailGenerator.cs
│   │   └── GifGenerator.cs
│   └── Security/
│       ├── JwtTokenService.cs
│       ├── PasswordHasher.cs
│       └── AuthorizationPolicies.cs
├── Common/
│   ├── Models/
│   ├── Exceptions/
│   ├── Extensions/
│   └── Validators/
└── Program.cs
```

## Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Username VARCHAR(100) UNIQUE NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(500) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP NOT NULL DEFAULT NOW()
);
```

### Videos Table
```sql
CREATE TABLE Videos (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Title VARCHAR(500) NOT NULL,
    Description TEXT,
    Provider VARCHAR(50) NOT NULL, -- 'OneDrive', 'Immich'
    ProviderId VARCHAR(500) NOT NULL, -- External provider's video ID
    ProviderPath VARCHAR(1000),
    Duration INTERVAL,
    FileSize BIGINT,
    MimeType VARCHAR(100),
    ThumbnailPath VARCHAR(500),
    GifPreviewPath VARCHAR(500),
    UserId UUID REFERENCES Users(Id) ON DELETE CASCADE,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UNIQUE(Provider, ProviderId, UserId)
);

CREATE INDEX idx_videos_user ON Videos(UserId);
CREATE INDEX idx_videos_provider ON Videos(Provider);
```

### RefreshTokens Table
```sql
CREATE TABLE RefreshTokens (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Token VARCHAR(500) UNIQUE NOT NULL,
    UserId UUID REFERENCES Users(Id) ON DELETE CASCADE,
    ExpiresAt TIMESTAMP NOT NULL,
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    RevokedAt TIMESTAMP
);

CREATE INDEX idx_refresh_tokens_user ON RefreshTokens(UserId);
CREATE INDEX idx_refresh_tokens_token ON RefreshTokens(Token);
```

## Setup Instructions

### 1. Create New Project
```bash
dotnet new webapi -n VideoManager.API -f net10.0
cd VideoManager.API
```

### 2. Install Required NuGet Packages
```bash
# Database & EF Core
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0

# Authentication & Security
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.0
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.2.0
dotnet add package BCrypt.Net-Next --version 4.0.3

# Provider SDKs
dotnet add package Microsoft.Graph --version 5.69.0
dotnet add package Microsoft.Identity.Client --version 4.68.0

# MediatR for CQRS pattern (optional but recommended for Vertical Slices)
dotnet add package MediatR --version 12.4.1

# Validation
dotnet add package FluentValidation.AspNetCore --version 11.3.0

# Video Processing (FFmpeg wrapper)
dotnet add package FFMpegCore --version 5.1.0

# Utilities
dotnet add package Swashbuckle.AspNetCore --version 6.8.1
```

> **Current Implementation Note**: The actual backend (`VManBackend`) uses:
> - Built-in .NET 10 OpenAPI with `.AddOpenApi()` and `.MapOpenApi()`
> - .NET Aspire for PostgreSQL orchestration (no manual connection string needed)
> - Custom CQRS implementation (not MediatR)
> - Kiota-generated Immich client
> - See `VManBackend/Program.cs` for actual configuration

### 3. Install FFmpeg
FFmpeg is required for video processing. Install via:

**Windows:**
```bash
# Using Chocolatey
choco install ffmpeg

# Or download from https://ffmpeg.org/download.html
```

**Linux:**
```bash
sudo apt-get install ffmpeg
```

**macOS:**
```bash
brew install ffmpeg
```

### 4. Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=videomanager;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "SecretKey": "your-super-secret-key-min-32-characters-long-for-security",
    "Issuer": "VideoManagerAPI",
    "Audience": "VideoManagerClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "VideoProviders": {
    "OneDrive": {
      "ClientId": "your-onedrive-client-id",
      "ClientSecret": "your-onedrive-client-secret",
      "TenantId": "common",
      "RedirectUri": "https://localhost:7000/auth/callback"
    },
    "Immich": {
      "BaseUrl": "https://your-immich-instance.com",
      "ApiKey": "your-immich-api-key"
    }
  },
  "VideoProcessing": {
    "ThumbnailWidth": 320,
    "ThumbnailHeight": 180,
    "ThumbnailTimeOffset": "00:00:03",
    "GifDuration": 5,
    "GifFps": 10,
    "GifWidth": 480,
    "StoragePath": "./storage"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 5. Database Migration
```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

## Implementation Guidelines

### Authentication Flow (JWT Bearer)
1. **Register**: User registers with username, email, password
2. **Login**: User logs in, receives access token (JWT) + refresh token
3. **Access Protected Resources**: Include `Authorization: Bearer {token}` header
4. **Refresh Token**: Use refresh token to get new access token when expired

### Vertical Slice Pattern Example
Each feature should contain:
- **Request/Response DTOs**
- **Validator** (FluentValidation)
- **Handler** (Business Logic)
- **Data Access** (EF Core queries)
- **Controller Endpoint**

Example structure for `GetVideos.cs`:
```csharp
namespace VideoManager.API.Features.Videos;

public static class GetVideos
{
    public record Query(int Page = 1, int PageSize = 20, string? Provider = null);
    
    public record Response(List<VideoDto> Videos, int TotalCount);
    
    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly ApplicationDbContext _db;
        
        public Handler(ApplicationDbContext db) => _db = db;
        
        public async Task<Response> Handle(Query request, CancellationToken ct)
        {
            // Implementation here
        }
    }
}
```

### Video Provider Integration
- **OneDrive**: Use Microsoft Graph SDK to list and stream videos
- **Immich**: Use HTTP client to call Immich API endpoints
- Implement `IVideoProvider` interface for each provider
- Support pagination and metadata retrieval

### Video Processing Best Practices
- Process thumbnails and GIFs asynchronously (background job/queue recommended)
- Store generated files in configured storage path
- Use FFmpeg efficiently with proper parameters
- Cache generated thumbnails/GIFs
- Handle video format compatibility

### Security Best Practices
1. **Password**: Hash with BCrypt (cost factor 12+)
2. **JWT**: 
   - Short-lived access tokens (15 min)
   - Secure refresh token storage
   - Include user claims (id, username, roles)
3. **HTTPS**: Enforce in production
4. **Rate Limiting**: Implement for authentication endpoints
5. **CORS**: Configure properly for your frontend
6. **Input Validation**: Use FluentValidation for all inputs

### Error Handling
- Use custom exception middleware
- Return consistent error responses
- Log errors appropriately
- Don't expose sensitive information in error messages

## Running the Application

### Development
```bash
dotnet run
```

### Production Build
```bash
dotnet publish -c Release -o ./publish
```

## API Endpoints Overview

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login and get tokens
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Revoke refresh token

### Videos
- `GET /api/videos` - List all videos (paginated)
- `GET /api/videos/{id}` - Get video details
- `POST /api/videos/import` - Import videos from provider
- `DELETE /api/videos/{id}` - Delete video
- `GET /api/videos/{id}/stream` - Stream video content

### Thumbnails
- `POST /api/videos/{id}/thumbnail` - Generate thumbnail
- `GET /api/videos/{id}/thumbnail` - Get thumbnail image

### Previews
- `POST /api/videos/{id}/preview` - Generate GIF preview
- `GET /api/videos/{id}/preview` - Get GIF preview

## Testing
- Unit tests for handlers
- Integration tests for API endpoints
- Mock external providers in tests

## Future Enhancements
- WebSocket support for real-time progress updates
- Video transcoding support
- Multiple thumbnail generation (timeline view)
- Video tagging and search
- Shared videos/playlists
- Role-based access control (Admin, User)
- Storage providers (AWS S3, Azure Blob)
