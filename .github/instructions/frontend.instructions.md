---
applyTo: 'video-manager-frontend/**/*.ts, video-manager-frontend/**/*.tsx'
name: Frontend Coding Style and Best Practices
description: This file describes the coding style and best practices for the Video Manager frontend codebase.
---

## Frontend Guidelines (video-manager-frontend/)

### Architecture: Next.js 15 App Router

#### Routing Structure
```
app/
├── (auth)/              # Auth routes (login, register)
│   ├── login/
│   └── register/
├── (dashboard)/         # Protected routes
│   ├── videos/
│   └── images/
├── layout.tsx           # Root layout
├── providers.tsx        # React Query & Theme providers
└── middleware.ts        # Route protection
```

### State Management Strategy
- **Client State** (Zustand): Auth state, UI preferences, dark mode
- **Server State** (React Query/TanStack Query): Videos, images, tags, users
- **Never duplicate server data in Zustand** - always use React Query


### API Integration
- Kiota-generated clients from backend OpenAPI specs found in ` https://localhost:<port>/openapi/v1.json`
- Custom wrappers in `lib/api/` for auth header injection
- **Note**: Currently using stub/mock data - backend integration in progress

### Styling Conventions
- Tailwind CSS 4 utility classes
- Use `cn()` helper for conditional classes
- Dark mode via `next-themes` (provider in `app/providers.tsx`)
- shadcn/ui components for consistency

## ❌ Don't Do This
- Storing server data in Zustand (use React Query)
- Client-side auth logic (use middleware)
- Direct fetch() calls (use generated API clients)
- Inline Tailwind classes without `cn()` helper
- Custom CSS files (use Tailwind utilities)
- Adding Playwright tests to Aspire AppHost (keep tests separate)
- Writing selectors/interactions directly in test files (use Page Object Model)

## File Naming Conventions

- Components: `{name}.tsx` (lowercase with dashes, e.g., `video-card.tsx`)
- Pages: Route folder structure (e.g., `app/(dashboard)/videos/page.tsx`)
- Hooks: `use{Name}.ts` (e.g., `useAuth.ts`)
- Types: `{name}.types.ts` or co-located with component
- Tests: `{name}.spec.ts` (e.g., `auth.spec.ts`, `videos.spec.ts`)
- Page Objects: `{name}.page.ts` (e.g., `login.page.ts`, `videos.page.ts`)

