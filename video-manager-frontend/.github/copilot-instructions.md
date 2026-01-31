# GitHub Copilot Instructions - Video Manager Frontend

## Project Context
This is a Next.js 15 frontend application for the Video Manager API. Built with TypeScript, Tailwind CSS, shadcn/ui components, React Query, Zustand, and Kiota-generated API client.

## Architecture Guidelines

### App Router Structure
- Use **App Router** (not Pages Router)
- Group routes with parentheses: `(auth)`, `(dashboard)`
- Server Components by default, Client Components when needed
- Use `'use client'` directive only when necessary (state, effects, interactivity)

### When to Use Client Components
- Forms and user input
- Event handlers (onClick, onChange)
- React hooks (useState, useEffect, useContext)
- Browser APIs (localStorage, window)
- Third-party libraries that use hooks

### When to Use Server Components
- Static content
- Data fetching (when not using React Query)
- SEO-critical pages
- Layout components without interactivity

## Coding Standards

### TypeScript
- **Strict mode** enabled
- Use **interfaces** for object shapes
- Use **types** for unions, intersections, primitives
- Avoid `any` - use `unknown` if type is truly unknown
- Export types with components when needed

### React Best Practices
- Use **functional components** exclusively
- Prefer **React Hooks** over class components
- Use **custom hooks** for reusable logic
- Keep components small and focused
- Extract complex logic into separate functions/hooks

### Naming Conventions
- **PascalCase**: Components, types, interfaces (`VideoCard`, `UserProfile`)
- **camelCase**: Functions, variables, hooks (`useVideos`, `handleSubmit`)
- **UPPER_SNAKE_CASE**: Constants (`API_BASE_URL`, `MAX_FILE_SIZE`)
- Prefix custom hooks with `use` (`useAuth`, `useVideos`)
- Prefix event handlers with `handle` (`handleClick`, `handleSubmit`)

## State Management

### React Query (TanStack Query)
- Use for **server state** (API data)
- Queries for GET requests: `useQuery`
- Mutations for POST/PUT/DELETE: `useMutation`
- Always provide query keys: `['videos', id]`
- Invalidate queries after mutations

```typescript
// Good: Proper query setup
const { data, isLoading, error } = useQuery({
  queryKey: ['videos', page, filters],
  queryFn: () => ApiClient.get('/api/videos'),
});

// Good: Mutation with cache invalidation
const mutation = useMutation({
  mutationFn: (data) => ApiClient.post('/api/videos', data),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['videos'] });
  },
});
```

### Zustand
- Use for **client state** (UI state, auth state)
- Keep stores small and focused
- Use `persist` middleware for localStorage
- Access outside components with `useStore.getState()`

```typescript
// Good: Focused auth store
const useAuthStore = create(persist(
  (set) => ({
    user: null,
    setUser: (user) => set({ user }),
    clearUser: () => set({ user: null }),
  }),
  { name: 'auth-storage' }
));
```

## Form Handling

### React Hook Form + Zod
- Always use **React Hook Form** for forms
- Validate with **Zod schemas**
- Use `zodResolver` for integration
- Handle errors gracefully

```typescript
// Good: Form with validation
const schema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
});

const { register, handleSubmit, formState: { errors } } = useForm({
  resolver: zodResolver(schema),
});
```

## Styling Guidelines

### Tailwind CSS
- Use **utility classes** - avoid custom CSS
- Use **responsive prefixes**: `md:`, `lg:`, `xl:`
- Use **shadcn/ui components** for consistency
- Use `cn()` utility for conditional classes

```typescript
import { cn } from '@/lib/utils';

// Good: Conditional styling
<div className={cn(
  "rounded-lg p-4",
  isActive && "bg-primary text-white",
  isDisabled && "opacity-50 cursor-not-allowed"
)} />
```

### shadcn/ui Components
- **Always** use shadcn/ui components when available
- Customize through variants, not direct styling
- Add new components: `npx shadcn@latest add <component>`
- Available components: button, card, input, dialog, badge, etc.

## API Integration

### Kiota-Generated Client
- Use **Kiota** for API client generation
- Regenerate client when API changes
- Wrap Kiota calls in custom hooks
- Handle errors consistently

```typescript
// Regenerate client
kiota generate -c kiota-config.json

// Use in custom hook
export function useVideos() {
  return useQuery({
    queryKey: ['videos'],
    queryFn: () => apiClient.videos.get(),
  });
}
```

### API Client Wrapper
- Use centralized `ApiClient` class
- Include auth tokens automatically
- Handle errors globally
- Return typed responses

## Error Handling

### Display Errors
- Use **sonner** for toast notifications
- Show user-friendly error messages
- Log detailed errors to console (dev only)
- Handle network errors gracefully

```typescript
import { toast } from 'sonner';

// Good: User-friendly error handling
mutation.mutate(data, {
  onSuccess: () => toast.success('Video uploaded!'),
  onError: (error) => {
    console.error(error);
    toast.error('Failed to upload video. Please try again.');
  },
});
```

## Performance Optimization

### Next.js Features
- Use **Image component** for images
- Use **Link component** for navigation
- Implement **loading.tsx** for loading states
- Implement **error.tsx** for error boundaries
- Use **metadata** for SEO

```typescript
// Good: Optimized image
import Image from 'next/image';

<Image 
  src={thumbnail}
  alt={title}
  width={320}
  height={180}
  className="rounded-md"
/>
```

### React Optimization
- Use **React.memo** for expensive components
- Use **useMemo** for expensive calculations
- Use **useCallback** for callback stability
- Avoid unnecessary re-renders

## Routing & Navigation

### App Router Patterns
- Use `useRouter` from `next/navigation`
- Use `redirect()` for server-side redirects
- Use `<Link>` for client-side navigation
- Use route groups for layouts

```typescript
import { useRouter } from 'next/navigation';
import Link from 'next/link';

// Client-side navigation
const router = useRouter();
router.push('/videos');

// Link component
<Link href="/videos">View Videos</Link>
```

### Middleware
- Protect routes in `middleware.ts`
- Check auth tokens
- Redirect unauthorized users
- Avoid heavy logic in middleware

## Component Structure

### Component Organization
```typescript
// 1. Imports
import { useState } from 'react';
import { Button } from '@/components/ui/button';

// 2. Types/Interfaces
interface VideoCardProps {
  video: Video;
  onDelete?: () => void;
}

// 3. Component
export function VideoCard({ video, onDelete }: VideoCardProps) {
  // 4. Hooks
  const [isDeleting, setIsDeleting] = useState(false);
  
  // 5. Event handlers
  const handleDelete = () => {
    setIsDeleting(true);
    onDelete?.();
  };
  
  // 6. Render
  return (
    <div>...</div>
  );
}
```

### File Structure
- One component per file
- Co-locate related components
- Export named exports (not default)
- Keep files under 300 lines

## Accessibility

### ARIA & Semantic HTML
- Use semantic HTML elements
- Add ARIA labels when needed
- Ensure keyboard navigation
- Use proper heading hierarchy

```typescript
// Good: Accessible button
<button
  onClick={handleSubmit}
  aria-label="Submit form"
  disabled={isLoading}
>
  {isLoading ? 'Submitting...' : 'Submit'}
</button>
```

## Environment Variables

### Next.js Env Vars
- Prefix client vars with `NEXT_PUBLIC_`
- Never expose secrets on client
- Use `.env.local` for local development
- Document all vars in `.env.example`

```env
# Good: Client-safe variable
NEXT_PUBLIC_API_URL=http://localhost:5000

# Bad: Exposes secret to client
NEXT_PUBLIC_API_SECRET=abc123
```

## Testing (Future)

### Testing Guidelines
- Unit tests for utilities/hooks
- Integration tests for components
- E2E tests for critical flows
- Use Testing Library patterns

## Common Patterns

### Loading States
```typescript
if (isLoading) return <LoadingSkeleton />;
if (error) return <ErrorMessage error={error} />;
if (!data) return null;

return <Content data={data} />;
```

### Conditional Rendering
```typescript
// Good: Early returns
if (!user) return <LoginPrompt />;

return <Dashboard user={user} />;

// Good: Ternary for simple cases
{isActive ? <ActiveIcon /> : <InactiveIcon />}

// Good: && for showing/hiding
{hasPermission && <AdminPanel />}
```

### Data Fetching
```typescript
// Good: Custom hook wrapping React Query
export function useVideos(filters) {
  return useQuery({
    queryKey: ['videos', filters],
    queryFn: () => ApiClient.get('/api/videos', { params: filters }),
  });
}
```

## Code Quality

### ESLint Rules
- Follow Next.js ESLint config
- Fix all linting errors before commit
- Use TypeScript strict mode
- No unused variables or imports

### Code Review Checklist
- [ ] TypeScript types are correct
- [ ] Components use proper hooks
- [ ] Forms have validation
- [ ] Errors are handled
- [ ] Loading states exist
- [ ] Accessibility considerations
- [ ] No console.logs in production
- [ ] Tailwind classes used properly
- [ ] shadcn/ui components used

## Git Workflow

### Commit Messages
Use conventional commits:
- `feat(auth): add login form`
- `fix(videos): correct thumbnail display`
- `refactor(hooks): simplify useAuth`
- `style(ui): update button variants`

## Resources

- [Next.js Documentation](https://nextjs.org/docs)
- [shadcn/ui](https://ui.shadcn.com)
- [React Query](https://tanstack.com/query/latest)
- [Zustand](https://zustand-demo.pmnd.rs)
- [Tailwind CSS](https://tailwindcss.com/docs)
- [React Hook Form](https://react-hook-form.com)
- [Zod](https://zod.dev)
