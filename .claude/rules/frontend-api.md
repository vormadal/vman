---
paths:
  - "video-manager-frontend/src/lib/api/**/*"
  - "video-manager-frontend/src/lib/hooks/**/*"
  - "video-manager-frontend/scripts/**/*"
---

# Frontend API Client Rules

## API Client

`src/lib/api/client.ts` is a hand-written singleton (`apiClient`) that wraps all backend HTTP calls. It handles auth token injection and ProblemDetails error parsing automatically.

- **Never** call `fetch()` directly in pages or hooks -- always use `apiClient` methods
- When the backend adds a new endpoint, add a corresponding method to `client.ts`
- Add request/response types to `src/lib/api/types.ts` alongside the new method
- `src/lib/api/generated/` contains Kiota-generated code run via `npm run generate:client` -- currently unused because the backend OpenAPI spec lacks typed response schemas; do not import from it

## Custom Hooks

All API calls must be wrapped in React Query hooks in `src/lib/hooks/`:

```typescript
// Existing hooks:
// useApi.ts     -- tags, items, sync, collections, people
// useAuth.ts    -- login, register, acceptInvite, completeProfile
// useAdmin.ts   -- admin users, invites, block/unblock, role changes
```

- Always use React Query for server state (caching, refetching, loading states)
- Never store API response data in Zustand stores
- Use the types from `src/lib/api/types.ts` for type safety
