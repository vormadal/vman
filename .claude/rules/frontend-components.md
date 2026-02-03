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
import { useVideos } from "@/lib/hooks/useVideos"

// Zustand store (client state only)
import { useAuthStore } from "@/lib/store/authStore"

// Styling utility
import { cn } from "@/lib/utils"
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
- Use shadcn/ui components - don't create custom equivalents
- Server state (API data) in React Query only
- Client state (UI, auth) in Zustand only
- Never duplicate server data in Zustand
- Use `'use client'` directive only when necessary (state, effects, interactivity)
