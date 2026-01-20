# Video Manager Frontend - Next.js Project Setup

## Project Overview
A modern video management frontend built with Next.js 15, TypeScript, Tailwind CSS, shadcn/ui components, and Kiota-generated API client for the VideoManager API.

## Technology Stack
- **Next.js 15** - React framework with App Router
- **TypeScript** - Type safety
- **Tailwind CSS** - Utility-first CSS framework
- **shadcn/ui** - High-quality UI components
- **Kiota** - API client generator from OpenAPI spec
- **React Query (TanStack Query)** - Server state management
- **Zustand** - Client state management
- **React Hook Form** - Form handling
- **Zod** - Schema validation

## Project Structure

```
video-manager-frontend/
├── src/
│   ├── app/
│   │   ├── (auth)/
│   │   │   ├── login/
│   │   │   │   └── page.tsx
│   │   │   └── register/
│   │   │       └── page.tsx
│   │   ├── (dashboard)/
│   │   │   ├── layout.tsx
│   │   │   ├── page.tsx
│   │   │   ├── videos/
│   │   │   │   ├── page.tsx
│   │   │   │   └── [id]/
│   │   │   │       └── page.tsx
│   │   │   ├── import/
│   │   │   │   └── page.tsx
│   │   │   └── settings/
│   │   │       └── page.tsx
│   │   ├── layout.tsx
│   │   ├── page.tsx
│   │   └── globals.css
│   ├── components/
│   │   ├── ui/ (shadcn components)
│   │   ├── auth/
│   │   │   ├── LoginForm.tsx
│   │   │   ├── RegisterForm.tsx
│   │   │   └── ProtectedRoute.tsx
│   │   ├── videos/
│   │   │   ├── VideoCard.tsx
│   │   │   ├── VideoGrid.tsx
│   │   │   ├── VideoPlayer.tsx
│   │   │   ├── VideoDetails.tsx
│   │   │   └── ThumbnailImage.tsx
│   │   ├── import/
│   │   │   ├── ProviderSelector.tsx
│   │   │   ├── OneDriveImport.tsx
│   │   │   └── ImmichImport.tsx
│   │   ├── layout/
│   │   │   ├── Header.tsx
│   │   │   ├── Sidebar.tsx
│   │   │   └── Footer.tsx
│   │   └── shared/
│   │       ├── LoadingSpinner.tsx
│   │       ├── ErrorBoundary.tsx
│   │       └── Pagination.tsx
│   ├── lib/
│   │   ├── api/ (Kiota-generated client)
│   │   ├── hooks/
│   │   │   ├── useAuth.ts
│   │   │   ├── useVideos.ts
│   │   │   ├── useThumbnails.ts
│   │   │   └── useImport.ts
│   │   ├── store/
│   │   │   └── authStore.ts
│   │   ├── utils/
│   │   │   ├── api-client.ts
│   │   │   ├── auth.ts
│   │   │   └── cn.ts
│   │   └── validations/
│   │       ├── auth.ts
│   │       └── video.ts
│   ├── types/
│   │   └── index.ts
│   └── middleware.ts
├── public/
│   ├── images/
│   └── icons/
├── .env.local
├── .env.example
├── next.config.js
├── tailwind.config.ts
├── tsconfig.json
├── package.json
├── kiota-config.json
└── README.md
```

## Setup Instructions

### 1. Create Next.js Project

```bash
# Navigate to project root
cd C:\workspaces\vormadal\vman

# Create Next.js app with TypeScript
npx create-next-app@latest video-manager-frontend --typescript --tailwind --app --src-dir --import-alias "@/*"

# Navigate to project
cd video-manager-frontend
```

When prompted, select:
- ✅ TypeScript
- ✅ ESLint
- ✅ Tailwind CSS
- ✅ `src/` directory
- ✅ App Router
- ✅ Import alias (@/*)
- ❌ Turbopack (optional)

### 2. Install Dependencies

```bash
# UI Components & Styling
npx shadcn@latest init

# When prompted:
# - Style: Default
# - Base color: Slate
# - CSS variables: Yes

# Install shadcn components (commonly used)
npx shadcn@latest add button card input label select textarea dialog dropdown-menu avatar badge separator tabs toast form

# State Management & Data Fetching
npm install @tanstack/react-query zustand

# Form Handling & Validation
npm install react-hook-form @hookform/resolvers zod

# API Client
npm install -g @microsoft/kiota

# Additional utilities
npm install clsx tailwind-merge class-variance-authority
npm install date-fns
npm install lucide-react

# Dev dependencies
npm install -D @types/node
```

### 3. Setup Kiota API Client

First, ensure your VideoManager API generates an OpenAPI specification:

```bash
# In VideoManager.API project, install Swashbuckle (if not already)
dotnet add package Swashbuckle.AspNetCore

# Run the API to generate swagger.json
# Then download from: https://localhost:7000/swagger/v1/swagger.json
```

Create `kiota-config.json`:
```json
{
  "version": "1.0.0",
  "clients": {
    "videoManagerApi": {
      "descriptionLocation": "http://localhost:5000/swagger/v1/swagger.json",
      "includePatterns": ["**/api/**"],
      "excludePatterns": [],
      "language": "typescript",
      "outputPath": "./src/lib/api",
      "clientClassName": "VideoManagerApiClient",
      "structuredMimeTypes": [
        "application/json"
      ]
    }
  }
}
```

Generate API client:
```bash
# Generate TypeScript client from OpenAPI spec
kiota generate -c kiota-config.json
```

### 4. Environment Variables

Create `.env.local`:
```env
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_APP_NAME=Video Manager
```

Create `.env.example`:
```env
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_APP_NAME=Video Manager
```

### 5. Configure Tailwind CSS

Update `tailwind.config.ts`:
```typescript
import type { Config } from "tailwindcss";

const config: Config = {
  darkMode: ["class"],
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "hsl(var(--primary))",
          foreground: "hsl(var(--primary-foreground))",
        },
        secondary: {
          DEFAULT: "hsl(var(--secondary))",
          foreground: "hsl(var(--secondary-foreground))",
        },
        destructive: {
          DEFAULT: "hsl(var(--destructive))",
          foreground: "hsl(var(--destructive-foreground))",
        },
        muted: {
          DEFAULT: "hsl(var(--muted))",
          foreground: "hsl(var(--muted-foreground))",
        },
        accent: {
          DEFAULT: "hsl(var(--accent))",
          foreground: "hsl(var(--accent-foreground))",
        },
        popover: {
          DEFAULT: "hsl(var(--popover))",
          foreground: "hsl(var(--popover-foreground))",
        },
        card: {
          DEFAULT: "hsl(var(--card))",
          foreground: "hsl(var(--card-foreground))",
        },
      },
      borderRadius: {
        lg: "var(--radius)",
        md: "calc(var(--radius) - 2px)",
        sm: "calc(var(--radius) - 4px)",
      },
    },
  },
  plugins: [require("tailwindcss-animate")],
};
export default config;
```

### 6. Setup Middleware for Auth Protection

Create `src/middleware.ts`:
```typescript
import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

const publicPaths = ['/login', '/register'];
const authPaths = ['/login', '/register'];

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const token = request.cookies.get('auth-token')?.value;

  // Redirect to login if accessing protected route without token
  if (!publicPaths.some(path => pathname.startsWith(path)) && !token) {
    return NextResponse.redirect(new URL('/login', request.url));
  }

  // Redirect to dashboard if accessing auth pages with valid token
  if (authPaths.some(path => pathname.startsWith(path)) && token) {
    return NextResponse.redirect(new URL('/videos', request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico).*)'],
};
```

### 7. Setup React Query Provider

Create `src/app/providers.tsx`:
```typescript
'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactNode, useState } from 'react';

export function Providers({ children }: { children: ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000, // 1 minute
            refetchOnWindowFocus: false,
          },
        },
      })
  );

  return (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
}
```

Update `src/app/layout.tsx`:
```typescript
import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { Providers } from "./providers";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "Video Manager",
  description: "Manage your videos from multiple providers",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
```

### 8. Create Auth Store (Zustand)

Create `src/lib/store/authStore.ts`:
```typescript
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface User {
  id: string;
  username: string;
  email: string;
}

interface AuthState {
  user: User | null;
  accessToken: string | null;
  refreshToken: string | null;
  setAuth: (user: User, accessToken: string, refreshToken: string) => void;
  clearAuth: () => void;
  isAuthenticated: () => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      setAuth: (user, accessToken, refreshToken) =>
        set({ user, accessToken, refreshToken }),
      clearAuth: () => set({ user: null, accessToken: null, refreshToken: null }),
      isAuthenticated: () => !!get().accessToken,
    }),
    {
      name: 'auth-storage',
    }
  )
);
```

### 9. Create API Client Utility

Create `src/lib/utils/api-client.ts`:
```typescript
import { useAuthStore } from '@/lib/store/authStore';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export class ApiClient {
  private static getHeaders(): HeadersInit {
    const { accessToken } = useAuthStore.getState();
    
    return {
      'Content-Type': 'application/json',
      ...(accessToken && { Authorization: `Bearer ${accessToken}` }),
    };
  }

  static async get<T>(endpoint: string): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'GET',
      headers: this.getHeaders(),
    });

    if (!response.ok) {
      throw new Error(`API Error: ${response.statusText}`);
    }

    return response.json();
  }

  static async post<T>(endpoint: string, data?: unknown): Promise<T> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'POST',
      headers: this.getHeaders(),
      body: data ? JSON.stringify(data) : undefined,
    });

    if (!response.ok) {
      throw new Error(`API Error: ${response.statusText}`);
    }

    return response.json();
  }

  static async delete(endpoint: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'DELETE',
      headers: this.getHeaders(),
    });

    if (!response.ok) {
      throw new Error(`API Error: ${response.statusText}`);
    }
  }
}
```

### 10. Create Validation Schemas

Create `src/lib/validations/auth.ts`:
```typescript
import { z } from 'zod';

export const loginSchema = z.object({
  username: z.string().min(3, 'Username must be at least 3 characters'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
});

export const registerSchema = z.object({
  username: z.string().min(3, 'Username must be at least 3 characters'),
  email: z.string().email('Invalid email address'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
  confirmPassword: z.string(),
}).refine((data) => data.password === data.confirmPassword, {
  message: "Passwords don't match",
  path: ["confirmPassword"],
});

export type LoginInput = z.infer<typeof loginSchema>;
export type RegisterInput = z.infer<typeof registerSchema>;
```

### 11. Create Custom Hooks

Create `src/lib/hooks/useAuth.ts`:
```typescript
import { useMutation } from '@tanstack/react-query';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/lib/store/authStore';
import { ApiClient } from '@/lib/utils/api-client';
import { LoginInput, RegisterInput } from '@/lib/validations/auth';

export function useLogin() {
  const router = useRouter();
  const setAuth = useAuthStore((state) => state.setAuth);

  return useMutation({
    mutationFn: async (credentials: LoginInput) => {
      return ApiClient.post<{
        user: { id: string; username: string; email: string };
        accessToken: string;
        refreshToken: string;
      }>('/api/auth/login', credentials);
    },
    onSuccess: (data) => {
      setAuth(data.user, data.accessToken, data.refreshToken);
      router.push('/videos');
    },
  });
}

export function useRegister() {
  const router = useRouter();

  return useMutation({
    mutationFn: async (data: RegisterInput) => {
      return ApiClient.post('/api/auth/register', data);
    },
    onSuccess: () => {
      router.push('/login');
    },
  });
}

export function useLogout() {
  const router = useRouter();
  const clearAuth = useAuthStore((state) => state.clearAuth);

  return () => {
    clearAuth();
    router.push('/login');
  };
}
```

Create `src/lib/hooks/useVideos.ts`:
```typescript
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ApiClient } from '@/lib/utils/api-client';

interface Video {
  id: string;
  title: string;
  description?: string;
  provider: string;
  thumbnailPath?: string;
  duration?: string;
  createdAt: string;
}

export function useVideos(page = 1, pageSize = 20, provider?: string) {
  return useQuery({
    queryKey: ['videos', page, pageSize, provider],
    queryFn: () =>
      ApiClient.get<{ videos: Video[]; totalCount: number }>(
        `/api/videos?page=${page}&pageSize=${pageSize}${provider ? `&provider=${provider}` : ''}`
      ),
  });
}

export function useVideo(id: string) {
  return useQuery({
    queryKey: ['video', id],
    queryFn: () => ApiClient.get<Video>(`/api/videos/${id}`),
    enabled: !!id,
  });
}

export function useDeleteVideo() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => ApiClient.delete(`/api/videos/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['videos'] });
    },
  });
}
```

## Component Examples

### Login Form Component
```typescript
// src/components/auth/LoginForm.tsx
'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useLogin } from '@/lib/hooks/useAuth';
import { loginSchema, LoginInput } from '@/lib/validations/auth';

export function LoginForm() {
  const { register, handleSubmit, formState: { errors } } = useForm<LoginInput>({
    resolver: zodResolver(loginSchema),
  });

  const loginMutation = useLogin();

  const onSubmit = (data: LoginInput) => {
    loginMutation.mutate(data);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <Label htmlFor="username">Username</Label>
        <Input id="username" {...register('username')} />
        {errors.username && (
          <p className="text-sm text-destructive">{errors.username.message}</p>
        )}
      </div>

      <div>
        <Label htmlFor="password">Password</Label>
        <Input id="password" type="password" {...register('password')} />
        {errors.password && (
          <p className="text-sm text-destructive">{errors.password.message}</p>
        )}
      </div>

      <Button type="submit" className="w-full" disabled={loginMutation.isPending}>
        {loginMutation.isPending ? 'Logging in...' : 'Login'}
      </Button>

      {loginMutation.isError && (
        <p className="text-sm text-destructive">Login failed. Please try again.</p>
      )}
    </form>
  );
}
```

## Running the Application

### Development
```bash
npm run dev
# Visit http://localhost:3000
```

### Build for Production
```bash
npm run build
npm start
```

### Linting
```bash
npm run lint
```

## Key Features to Implement

1. **Authentication**
   - Login/Register forms
   - Protected routes
   - Token refresh logic
   - Logout functionality

2. **Video Management**
   - Video grid/list view
   - Video details page
   - Video player
   - Thumbnail display
   - GIF preview

3. **Video Import**
   - Provider selection (OneDrive, Immich)
   - OAuth flow for OneDrive
   - API key configuration for Immich
   - Import progress indication

4. **Settings**
   - User profile management
   - Provider configuration
   - Theme toggle (dark/light mode)

## Best Practices

- Use Server Components where possible for better performance
- Client Components only when needed (forms, interactivity)
- Implement error boundaries
- Add loading states
- Use optimistic updates for better UX
- Implement proper TypeScript types
- Follow accessibility guidelines
- Add proper SEO metadata

## Additional Recommendations

1. **Add Loading States**: Implement skeleton loaders using shadcn/ui
2. **Error Handling**: Create toast notifications for user feedback
3. **Dark Mode**: Implement theme toggle with next-themes
4. **Infinite Scroll**: For video grid using Intersection Observer
5. **Video Upload**: Add drag-and-drop file upload
6. **Search & Filter**: Implement video search and filtering
7. **Responsive Design**: Ensure mobile-friendly layouts
