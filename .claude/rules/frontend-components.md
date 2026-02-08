---
paths: "video-manager-frontend/**/*.tsx"
---

# Frontend Component Rules

## Imports Pattern

```typescript
// shadcn/ui components
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogTrigger } from "@/components/ui/dialog"

// Custom hooks
import { useAuth } from "@/lib/hooks/useAuth"
import { useImages } from "@/lib/hooks/useImages"

// Zustand store (client state only)
import { useAuthStore } from "@/lib/store/authStore"

// Styling utility
import { cn } from "@/lib/utils"

// Toast notifications
import { useToast } from "@/hooks/use-toast"
```

## Form Validation

Define Zod schemas in `lib/validations/`, not inline:

```typescript
import { zodResolver } from "@hookform/resolvers/zod"
import { useForm } from "react-hook-form"
import { loginSchema } from "@/lib/validations/auth"

const form = useForm({
  resolver: zodResolver(loginSchema),
})
```

## Rules

- Use `cn()` helper for all conditional Tailwind classes
- Use shadcn/ui components -- don't create custom equivalents
- Server state (API data) in React Query only
- Client state (UI, auth) in Zustand only -- stores: `authStore`, `collectionModeStore`
- Never duplicate server data in Zustand
- Use `'use client'` directive only when necessary (state, effects, interactivity)
- Format dates as `dd-MM-yyyy` and time as 24H `HH:mm`
- **Always** use generated API client (`lib/api/client.ts`) to fetch data
- **Never** make manual changes to `lib/api/client.ts` -- use `npm run generate:client`

## App Router Structure

```
src/app/
├── (auth)/              # Public: login, register, accept-invite
├── (dashboard)/         # Protected routes (admin, collections, items, sync, tags)
├── complete-profile/    # Profile completion after invite
└── middleware.ts        # Route protection
```
