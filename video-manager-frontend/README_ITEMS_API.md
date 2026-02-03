# Using the Items API

The backend API is now ready to serve items from Immich! The TypeScript client has been generated with support for the new `untagged` parameter.

## Prerequisites

1. **Backend must be running** (Aspire with VManBackend)
2. **Immich integration configured** (IMMICH_API_KEY environment variable set)
3. **Authentication token** (login to get JWT token)

## API Client Generation

The Kiota client is auto-generated from the backend's OpenAPI spec:

```bash
npm run generate:client
```

This creates TypeScript types in `src/lib/api/generated/`

## Available Query Parameters

```typescript
interface GetItemsParams {
  provider?: string        // Default: "immich"
  type?: 'image' | 'video' | 'audio' | 'other'
  isFavorite?: boolean
  untagged?: boolean       // NEW: Filter to only show items without tags
  sortBy?: string          // Default: "createdAt"
  sortDescending?: boolean // Default: true
  page?: number            // Default: 1
  pageSize?: number        // Default: 50, max: 100
}
```

## Usage Examples

### 1. Get All Items

```typescript
import { createApiClient } from '@/lib/api/kiota-client'

const client = createApiClient()
const response = await client.api.items.get()

console.log('Total:', response.totalCount)
console.log('Items:', response.items)
```

### 2. Get Untagged Videos (NEW!)

```typescript
const untaggedVideos = await client.api.items.get({
  queryParameters: {
    untagged: true,
    type: 'video',
    page: 1,
    pageSize: 20
  }
})

console.log(`Found ${untaggedVideos.items.length} untagged videos`)
```

### 3. Get Favorite Images

```typescript
const favorites = await client.api.items.get({
  queryParameters: {
    type: 'image',
    isFavorite: true,
    sortBy: 'createdAt',
    sortDescending: true
  }
})
```

### 4. Pagination

```typescript
const page2 = await client.api.items.get({
  queryParameters: {
    page: 2,
    pageSize: 50
  }
})

console.log(`Page ${page2.page} of ${Math.ceil(page2.totalCount / page2.pageSize)}`)
```

## Response Format

```typescript
interface ItemsResponse {
  items: ItemDto[]
  totalCount: number
  page: number
  pageSize: number
}

interface ItemDto {
  provider: string          // "immich"
  id: string               // Provider-specific ID
  name: string             // Filename
  type: 'image' | 'video' | 'audio' | 'other'
  createdAt: string        // ISO 8601 timestamp
  thumbnailUrl?: string    // e.g., "/api/immich/assets/{id}/thumbnail"
  previewUrl?: string
  isFavorite: boolean
  tags: TagDto[]           // Tags associated with this item
}

interface TagDto {
  id: string               // UUID
  name: string
}
```

## Performance Notes

- **Caching**: Provider responses are cached for 5 minutes in memory
- **Untagged Filter**: Fetches up to 500 items from provider, filters locally, then paginates
- **Tag Lookup**: Uses batch query (single DB roundtrip) for efficiency

## Testing

You can test the API using curl:

```bash
# Login to get token
TOKEN=$(curl -s -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test.user@example.com","password":"TestPassword123!"}' \
  | jq -r '.token')

# Get untagged videos
curl -s "http://localhost:5001/api/items?untagged=true&type=video" \
  -H "Authorization: Bearer $TOKEN" \
  | jq '.items | length'
```

## Next Steps

1. Create a React component to display items
2. Add filtering UI (dropdown for type, checkbox for untagged)
3. Implement infinite scroll pagination
4. Add thumbnail rendering

See `src/lib/api/examples/items-example.ts` for more examples.
