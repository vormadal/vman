# Video Manager Frontend

A modern video management frontend built with Next.js 15, TypeScript, Tailwind CSS, and shadcn/ui components.

## Current Status

✅ **Frontend Complete** - Authentication UI, video browsing layout, and API client generation ready  
⚠️ **Backend Available** - API running via .NET Aspire with OpenAPI spec (no endpoints yet)

## Features

- 🎬 **Video Browsing** - Grid view of videos with thumbnails
- 🏷️ **Tag Management** - Add, remove, and filter videos by tags  
- 🔐 **Authentication** - Login/Register with JWT
- 🤖 **Auto API Client** - Automatically generates TypeScript client from backend OpenAPI spec
- 🎨 Modern UI with shadcn/ui components
- 🔄 State management with React Query & Zustand
- 📱 Responsive design with Tailwind CSS
- 🚀 Built with Next.js 15 App Router

## Tech Stack

- **Framework**: Next.js 15
- **Language**: TypeScript
- **Styling**: Tailwind CSS 4
- **UI Components**: shadcn/ui
- **State Management**: Zustand + React Query
- **Forms**: React Hook Form + Zod
- **API Client**: Kiota (auto-generated from OpenAPI)

## Getting Started

### Prerequisites

- Node.js 18+ installed
- Backend API running (see `../VideoManager/ASPIRE_README.md`)

### Installation

```bash
# Install dependencies
npm install

# Run development server (auto-generates client first)
npm run dev
```

Visit [http://localhost:3000](http://localhost:3000)

## API Client Generation

The frontend automatically generates a TypeScript API client from the backend's OpenAPI specification.

### Available Scripts

```bash
# Manually generate client (smart - only if changed)
npm run generate:client

# Watch for spec changes and auto-regenerate
npm run generate:watch

# Run dev server with fresh client generation
npm run dev:with-gen

# Run dev (generates client automatically via predev hook)
npm run dev
```

### How It Works

1. **Smart Detection**: Compares OpenAPI spec hash to detect changes
2. **Auto-Generate**: Only regenerates if spec changed or client missing
3. **Backend Check**: Verifies backend is running before attempting generation
4. **Watch Mode**: Continuously monitors backend for spec changes

### Manual Generation

```bash
# Generate client from running backend
npm run generate:client
```

Output:
```
🚀 Smart API Client Generator

ℹ️  Checking backend API availability...
✅ Backend API is running (OpenAPI 3.1.1)
ℹ️  API: VManBackend | v1 v1.0.0
✅ Found 0 endpoint(s) in OpenAPI spec
ℹ️  OpenAPI spec has changed

📦 Generating TypeScript client...
✅ Client generated successfully!
✨ Generated client available at: ./src/lib/api/generated
```

### Watch Mode

For active backend development, use watch mode to auto-regenerate on changes:

```bash
npm run generate:watch
```

This will continuously monitor the backend and regenerate the client whenever the OpenAPI spec changes.

## Project Structure

```
src/
├── app/                    # Next.js app router pages
│   ├── (dashboard)/       # Main application pages
│   │   ├── images/       # Image browsing page
│   │   └── videos/       # (Legacy video page)
│   ├── layout.tsx
│   └── providers.tsx
├── components/            # React components
│   ├── ui/               # shadcn/ui components
│   └── ...
├── lib/                  # Utilities and hooks
│   ├── api/             # Mock API implementations
│   │   ├── mockData.ts  # Stub image and tag data
│   │   └── imageApi.ts  # Stub API functions
│   ├── hooks/           # Custom React hooks
│   │   └── useImages.ts # React Query hooks for images
│   ├── store/           # Zustand stores
│   └── utils/           # Utility functions
└── middleware.ts        # Next.js middleware (no auth required)
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm start` - Start production server
- `npm run lint` - Run ESLint

## Current Implementation

### Mock Data
The app currently uses stub data located in `src/lib/api/mockData.ts`:
- 6 sample images with Unsplash photos
- 14 pre-defined tags with colors
- Mock API functions that simulate async behavior

### Features Implemented
- ✅ Image grid with thumbnails
- ✅ Tag filtering
- ✅ Add tags to images
- ✅ Remove tags from images
- ✅ Create new tags
- ✅ Color-coded tag badges

### When Backend is Ready

Once the backend API is ready:

1. **Replace mock API** in `src/lib/api/imageApi.ts` with real API calls
2. **Update hooks** in `src/lib/hooks/useImages.ts` to use the real API client
3. **Add authentication** if needed (middleware, auth pages already exist in codebase)

Example API structure to implement:
```typescript
// GET /api/images?page=1&pageSize=20&tag=nature
// POST /api/images/:id/tags
// DELETE /api/images/:id/tags/:tagId
// GET /api/tags
// POST /api/tags
```

## Features to Implement

- [ ] Image upload
- [ ] Image detail view
- [ ] Tag color picker
- [ ] Search functionality
- [ ] Pagination
- [ ] Dark mode toggle
- [ ] Real backend integration

## Learn More

- [Next.js Documentation](https://nextjs.org/docs)
- [shadcn/ui Documentation](https://ui.shadcn.com)
- [React Query Documentation](https://tanstack.com/query/latest)

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme).

