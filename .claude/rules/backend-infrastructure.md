---
paths:
  - "**/Infrastructure/**/*.cs"
---

# Backend Infrastructure

## Immich Integration (`Infrastructure/Immich/`)

- `IImmichService` / `ImmichService`: Interface and implementation for Immich API
- `StubImmichService`: Mock implementation for development (enabled via `USE_STUB_IMMICH=true`)
- `Generated/`: Kiota-generated client from Immich OpenAPI spec -- do NOT edit manually

## Media Providers (`Infrastructure/Providers/`)

- `IMediaProvider`: Abstraction over media sources (currently only Immich)
- `ImmichMediaProvider`: Immich-specific implementation
- `CachedMediaProvider`: Decorator that wraps any provider with `IMemoryCache`

## Background Sync (`Infrastructure/Sync/`)

- `SyncChannel`: In-memory channel for queuing sync requests
- `SyncBackgroundService`: `IHostedService` that processes jobs from the channel
- `ImmichSyncProcessor`: Actual sync logic (fetches assets, people from Immich)

## Authentication (`Infrastructure/Authentication/`)

- `IJwtService` / `JwtService`: JWT token generation and validation
- Symmetric key signing, configured via `Jwt:SecretKey` in appsettings

## Data (`Infrastructure/Data/`)

- `DbSeeder`: Seeds admin user (all envs) and test user (dev only)
