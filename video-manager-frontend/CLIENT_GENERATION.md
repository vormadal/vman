# Automatic API Client Generation

This guide explains the smart API client generation system for the Video Manager frontend.

## Overview

The frontend uses **Kiota** to automatically generate a type-safe TypeScript client from the backend's OpenAPI specification. The system includes intelligent caching and change detection to minimize unnecessary regeneration.

## Architecture

```
┌─────────────────────┐
│  Backend API        │
│  (VManBackend)      │
│  Port: 7213         │
└──────────┬──────────┘
           │ Exposes
           ▼
┌─────────────────────┐
│  OpenAPI Spec       │
│  /openapi/v1.json   │
└──────────┬──────────┘
           │ Downloads & Hashes
           ▼
┌─────────────────────┐
│  Smart Generator    │
│  (generate-client)  │
└──────────┬──────────┘
           │ Only if changed
           ▼
┌─────────────────────┐
│  Kiota CLI          │
│  (@microsoft/kiota) │
└──────────┬──────────┘
           │ Generates
           ▼
┌─────────────────────┐
│  TypeScript Client  │
│  src/lib/api/       │
│  generated/         │
└─────────────────────┘
```

## Smart Generation Features

### 1. Change Detection
The generator uses SHA-256 hashing to detect OpenAPI spec changes:
- Downloads spec from `https://localhost:7213/openapi/v1.json`
- Computes hash of spec content
- Compares with cached hash in `.kiota-cache/spec-hash.txt`
- Only regenerates if hash differs

### 2. Backend Availability Check
Before attempting generation:
- Pings backend API with retry logic (3 attempts, 2s delay)
- Handles SSL certificate issues (accepts self-signed certs)
- Provides helpful error messages if backend is down

### 3. Cache Management
```
.kiota-cache/
├── openapi-spec.json    # Last downloaded spec
└── spec-hash.txt        # SHA-256 hash of spec
```

These files prevent unnecessary regeneration when spec hasn't changed.

### 4. Automatic Installation
- Checks for Kiota CLI availability
- Auto-installs `@microsoft/kiota` if missing
- Uses `npx` for zero global installation

## NPM Scripts

### `npm run generate:client`
**Smart one-time generation**

```bash
npm run generate:client
```

Behavior:
1. ✅ Checks if backend is running
2. 📥 Downloads OpenAPI spec
3. 🔍 Compares with cached version
4. ⏭️ Skips if no changes detected
5. 📦 Generates client only if needed
6. 💾 Caches new spec and hash

### `npm run generate:watch`
**Continuous monitoring**

```bash
npm run generate:watch
```

Behavior:
- 👀 Polls backend every 5 seconds
- 🔄 Auto-regenerates on spec changes
- ⚡ Detects backend restart
- ⚠️ Shows warnings when backend is unavailable

Ideal for:
- Active backend development
- Multiple developers working on API
- Immediate feedback on spec changes

### `npm run dev`
**Development with auto-generation**

```bash
npm run dev
```

Behavior:
- Runs `generate:client` as predev hook
- Ensures client is fresh before starting dev server
- Fails fast if backend is unavailable

### `npm run dev:with-gen`
**Explicit generation before dev**

```bash
npm run dev:with-gen
```

Same as `npm run dev` but more explicit.

## Configuration

### Kiota Config (`kiota-config.json`)
```json
{
  "version": "1.0.0",
  "clients": {
    "videoManagerApi": {
      "descriptionLocation": "https://localhost:7213/openapi/v1.json",
      "includePatterns": ["**"],
      "excludePatterns": [],
      "language": "typescript",
      "outputPath": "./src/lib/api/generated",
      "clientClassName": "VideoManagerApiClient",
      "structuredMimeTypes": ["application/json"]
    }
  }
}
```

### Generator Config (`scripts/generate-client.js`)
```javascript
const CONFIG = {
  openApiUrl: 'https://localhost:7213/openapi/v1.json',
  outputDir: './src/lib/api/generated',
  cacheDir: './.kiota-cache',
  maxRetries: 3,
  retryDelay: 2000,
};
```

## Workflows

### First Time Setup
```bash
# 1. Install dependencies
npm install

# 2. Start backend (in separate terminal)
cd ../VideoManager/VideoManager.AppHost
aspire run

# 3. Generate client
npm run generate:client

# 4. Start frontend
npm run dev
```

### Daily Development
```bash
# Option 1: Auto-generate on dev start
npm run dev

# Option 2: Watch for changes (separate terminal)
npm run generate:watch
# Then in another terminal:
npm run dev
```

### Backend API Changes
When you add new endpoints to the backend:

1. **Automatic** (if using watch mode):
   - Watch detects change
   - Client regenerates automatically
   - TypeScript shows new types immediately

2. **Manual**:
   ```bash
   npm run generate:client
   ```

## Troubleshooting

### Backend Not Running
```
❌ Backend API is not reachable: connect ECONNREFUSED
⚠️  Make sure the backend is running:
  cd VideoManager\VideoManager.AppHost
  aspire run
```

**Solution**: Start the Aspire AppHost first.

### SSL Certificate Error
The generator automatically accepts self-signed certificates, but if you see SSL errors:

```bash
# Trust Aspire development certificates
cd ../VideoManager/VideoManager.AppHost
dotnet dev-certs https --trust
```

### Empty Spec (No Endpoints)
```
⚠️  OpenAPI spec has no endpoints defined yet
ℹ️  Endpoints will appear after implementing controllers
```

This is normal for a new backend. The client will be generated with base types only.

### Kiota Installation Fails
```bash
# Manually install Kiota
npm install --save-dev @microsoft/kiota

# Or globally
npm install -g @microsoft/kiota
```

### Client Not Updating
```bash
# Clear cache and force regeneration
rm -rf .kiota-cache
rm -rf src/lib/api/generated
npm run generate:client
```

## Using the Generated Client

### Import Client
```typescript
import { VideoManagerApiClient } from '@/lib/api/generated';
```

### Create Client Instance
```typescript
const client = new VideoManagerApiClient({
  baseUrl: 'https://localhost:7213',
  // Add authentication header
  requestAdapter: {
    // Configure auth token
  }
});
```

### Make API Calls
```typescript
// Example: Get videos (when endpoint exists)
const videos = await client.videos.get();
```

## Benefits

✅ **Type Safety** - Full TypeScript support for API calls  
✅ **Auto-Sync** - Client stays in sync with backend  
✅ **Developer Experience** - No manual client code  
✅ **Smart Caching** - Only regenerates when needed  
✅ **Error Handling** - Clear messages for common issues  
✅ **Watch Mode** - Continuous development workflow

## Advanced Usage

### Custom Watch Interval
Edit `scripts/watch-spec.js`:
```javascript
const POLL_INTERVAL = 3000; // 3 seconds instead of 5
```

### Different Backend URL
Edit `scripts/generate-client.js`:
```javascript
const CONFIG = {
  openApiUrl: 'https://api.example.com/openapi/v1.json',
  // ...
};
```

Also update `kiota-config.json` to match.

### Pre-commit Hook
Add to `package.json`:
```json
{
  "husky": {
    "hooks": {
      "pre-commit": "npm run generate:client"
    }
  }
}
```

## CI/CD Integration

### GitHub Actions Example
```yaml
- name: Generate API Client
  run: |
    # Start backend in background
    cd VideoManager/VideoManager.AppHost
    dotnet run &
    
    # Wait for backend to be ready
    sleep 10
    
    # Generate client
    cd ../../video-manager-frontend
    npm run generate:client
    
    # Build frontend
    npm run build
```

## Files Created

```
video-manager-frontend/
├── scripts/
│   ├── generate-client.js   # Smart generator
│   └── watch-spec.js         # Watch mode
├── .kiota-cache/             # Cache directory
│   ├── openapi-spec.json     # Cached spec
│   └── spec-hash.txt         # Spec hash
├── src/lib/api/generated/    # Generated client
│   ├── index.ts
│   ├── models/
│   └── (Kiota output)
├── kiota-config.json         # Kiota config
└── package.json              # NPM scripts
```

## Next Steps

1. ✅ Setup complete - client generation ready
2. 🔨 Implement backend controllers
3. 🔄 Client auto-updates with new endpoints
4. 🎯 Use type-safe client in frontend components

Happy coding! 🚀
