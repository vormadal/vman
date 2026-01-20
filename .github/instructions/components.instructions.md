---
applyTo: '**/*.tsx'
name: 'React Components Instructions'
---


## Component Patterns
```typescript
// Use shadcn/ui components
import { Button } from "@/components/ui/button"
import { Dialog, DialogContent, DialogTrigger } from "@/components/ui/dialog"

// Custom hooks
import { useAuth } from "@/lib/hooks/useAuth"
import { useVideos } from "@/lib/hooks/useVideos"

// Zustand store
import { useAuthStore } from "@/lib/store/authStore"
```

### Form Validation
- **React Hook Form** + **Zod** for type-safe validation
- Define schema in `lib/validations/`
- Use resolver integration:
```typescript
import { zodResolver } from "@hookform/resolvers/zod"
import { useForm } from "react-hook-form"
import { loginSchema } from "@/lib/validations/auth"

const form = useForm({
  resolver: zodResolver(loginSchema),
})
```