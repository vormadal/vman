---
paths:
  - "**/Common/Models/**/*.cs"
  - "**/Common/Data/**/*.cs"
  - "**/Migrations/**/*.cs"
---

# Database & Models

## EF Core Migrations

```bash
cd VideoManager/VManBackend
dotnet ef migrations add {MigrationName}
dotnet ef database update
```

Migrations run automatically on startup in development. Do NOT run `dotnet ef database update` manually against the development database -- Aspire handles this.

## Models

Key entities in `VManBackend/Common/Models/`:
- **User**: Email, hashed password, display name, role (User/Admin), blocked status
- **UserInvite**: Token-based invite system for new users
- **Tag**: Global tags for categorizing items
- **Item**: Media items synced from Immich (provider + external ID)
- **ItemTag**: Junction table (Item-Tag M:N)
- **Collection**: User-created ordered collections of items
- **CollectionItem**: Junction table with sort order
- **Person**: Face recognition data (synced from Immich)
- **ItemPerson**: Junction table (Item-Person M:N)
- **SyncJob**: Tracks background sync operations with status
- **SyncHistory**: Historical sync records
- **ImmichAsset** / **ImmichExifData**: Immich-specific asset metadata

## Rules

- Use `DateTimeOffset` for all timestamps (not DateTime)
- Configure relationships in `ApplicationDbContext` using Fluent API
- Use Guid primary keys
- BCrypt for password hashing (via `BCrypt.Net-Next`)
