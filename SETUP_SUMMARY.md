# Video Manager Project - Setup Complete

## Project Structure

```
vman/
├── video-manager-frontend/          # Next.js 15 Frontend
│   ├── src/
│   │   ├── app/                     # App Router pages
│   │   │   ├── (auth)/             # Login, Register
│   │   │   ├── (dashboard)/        # Videos page
│   │   │   ├── layout.tsx
│   │   │   └── providers.tsx
│   │   ├── components/
│   │   │   ├── ui/                 # shadcn/ui components
│   │   │   ├── auth/               # LoginForm, RegisterForm
│   │   │   ├── videos/             # Video components (to be added)
│   │   │   ├── layout/             # Layout components (to be added)
│   │   │   └── shared/             # Shared components (to be added)
│   │   ├── lib/
│   │   │   ├── api/                # Kiota-generated client
│   │   │   ├── hooks/              # useAuth, useVideos
│   │   │   ├── store/              # authStore (Zustand)
│   │   │   ├── utils/              # api-client, cn
│   │   │   └── validations/        # Zod schemas
│   │   └── middleware.ts           # Auth protection
│   ├── .github/
│   │   └── copilot-instructions.md # Frontend Copilot instructions
│   ├── .env.local                  # Environment variables
│   ├── kiota-config.json          # API client config
│   ├── package.json
│   └── README.md
│
├── VideoManager/                   # .NET 10 Solution
│   ├── VideoManager.AppHost/       # Aspire orchestration
│   ├── VideoManager.ServiceDefaults/ # Shared Aspire config
│   └── VManBackend/                # Main API project
│
├── .github/
│   └── copilot-instructions.md    # Backend Copilot instructions
├── PROJECT_SETUP.md               # Backend setup instructions
└── FRONTEND_SETUP.md              # Frontend setup instructions
```

## What Has Been Created

### Frontend (video-manager-frontend)
✅ **Next.js 15 Application** with TypeScript
✅ **Tailwind CSS** configured
✅ **shadcn/ui** components installed (button, card, input, label, select, dialog, badge, sonner)
✅ **React Query** for server state management
✅ **Zustand** for client state management
✅ **React Hook Form + Zod** for form validation
✅ **Authentication system**
   - Login page (`/login`)
   - Register page (`/register`)
   - Auth store with persistence
   - Middleware for route protection
✅ **Videos page** (`/videos`) with basic layout
✅ **Custom hooks** (useAuth, useVideos)
✅ **API client wrapper** with auth token handling
✅ **Environment configuration** (.env.local, .env.example)
✅ **Kiota configuration** (kiota-config.json)
✅ **Copilot instructions** for frontend development

### Documentation
✅ **PROJECT_SETUP.md** - Complete backend setup guide
✅ **FRONTEND_SETUP.md** - Complete frontend setup guide
✅ **.github/copilot-instructions.md** - Backend Copilot guidelines
✅ **video-manager-frontend/.github/copilot-instructions.md** - Frontend Copilot guidelines
✅ **README.md** - Frontend project documentation

## Next Steps

### 1. Backend Development (VManBackend API)
The backend is already set up with .NET Aspire. To run:
```bash
# Navigate to AppHost project
cd VideoManager\VideoManager.AppHost

# Run Aspire (starts all services including PostgreSQL)
aspire run

# Or use dotnet directly
dotnet run
```

This will:
- Start PostgreSQL in a container with pgAdmin
- Start the VManBackend API
- Start the Next.js frontend
- Open the Aspire dashboard showing all services, logs, and metrics

**OpenAPI Spec**: Available at `https://localhost:7213/openapi/v1.json`

### 2. Database Setup
- Install PostgreSQL
- Create database: `videomanager`
- Update connection string in `appsettings.json`
- Run migrations: `dotnet ef database update`

### 3. Frontend Development
```bash
cd video-manager-frontend

# Start development server
npm run dev

# Visit http://localhost:3000
```

### 4. Generate API Client with Kiota
Once the backend is running with OpenAPI:
```bash
# Install Kiota globally
npm install -g @microsoft/kiota

# Generate TypeScript client from running API
cd video-manager-frontend
kiota generate -c kiota-config.json

# Or specify the OpenAPI URL directly
kiota generate -l typescript -d https://localhost:7213/openapi/v1.json -o src/lib/api/generated
```

**Note**: The backend uses `.AddOpenApi()` and `.MapOpenApi()`, not Swagger. The OpenAPI spec is available at `/openapi/v1.json` (not `/swagger/v1/swagger.json`).

## Key Features to Implement

### Backend (.NET 10 API)
- [x] Aspire orchestration setup
- [x] PostgreSQL database integration
- [x] OpenAPI documentation (v3.1.1)
- [x] JWT Authentication infrastructure
- [x] Immich client integration (Kiota-generated)
- [ ] User authentication endpoints
- [ ] Video management endpoints
- [ ] OneDrive provider integration
- [ ] FFmpeg thumbnail generation
- [ ] FFmpeg GIF preview generation
- [ ] Database migrations (schema design complete)

### Frontend (Next.js)
- [x] Authentication UI (Login/Register)
- [x] Protected routes
- [x] Basic video listing page
- [x] **Smart API client generation** (auto-generates from OpenAPI spec)
- [x] Watch mode for continuous client regeneration
- [ ] Video detail page with player
- [ ] Video import from providers
- [ ] Thumbnail display
- [ ] GIF preview display
- [ ] Search and filtering
- [ ] Pagination
- [ ] Dark mode toggle
- [ ] User settings

## Technology Stack Summary

### Backend
- .NET 10 MVC
- PostgreSQL + Entity Framework Core
- JWT Authentication
- FFmpeg (video processing)
- Microsoft Graph SDK (OneDrive)
- Vertical Slice Architecture

### Frontend
- Next.js 15 (App Router)
- TypeScript
- Tailwind CSS
- shadcn/ui
- React Query (TanStack Query)
- Zustand
- React Hook Form + Zod
- Kiota (API client)

## Running the Full Stack

### Option 1: Run with Aspire (Recommended)
```bash
cd VideoManager\VideoManager.AppHost
aspire run
```
This starts everything:
- **Backend API**: https://localhost:5000
- **Frontend**: http://localhost:3000
- **PostgreSQL + pgAdmin**: Automatically provisioned
- **Aspire Dashboard**: https://localhost:17037

### Option 2: Run Separately
1. **Start PostgreSQL database** (if not using Aspire)
2. **Start Backend API**:
   ```bash
   cd VideoManager\VManBackend
   dotnet run
   # API runs on http://localhost:5000
   ```
3. **Start Frontend**:
   ```bash
   cd video-manager-frontend
   npm run dev
   # Frontend runs on http://localhost:3000
   ```

## Environment Variables

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=videomanager;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "SecretKey": "your-super-secret-key-min-32-characters-long",
    "Issuer": "VideoManagerAPI",
    "Audience": "VideoManagerClient"
  }
}
```

### Frontend (.env.local)
```env
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_APP_NAME=Video Manager
```

## Development Workflow

### Quick Start
```bash
# Terminal 1: Start backend with Aspire
cd VideoManager\VideoManager.AppHost
aspire run

# Terminal 2: Start frontend (auto-generates client)
cd video-manager-frontend
npm run dev
```

### With API Client Watch Mode
For active backend development:
```bash
# Terminal 1: Backend
cd VideoManager\VideoManager.AppHost
aspire run

# Terminal 2: Watch for API changes
cd video-manager-frontend
npm run generate:watch

# Terminal 3: Frontend dev server
cd video-manager-frontend
npm run dev
```

The watch mode will automatically regenerate the TypeScript client whenever the backend's OpenAPI spec changes.

### Manual Client Generation
```bash
cd video-manager-frontend
npm run generate:client
```

See `video-manager-frontend/CLIENT_GENERATION.md` for detailed documentation.

## Development Best Practices

1. **Backend changes**: Make API changes, client auto-regenerates
2. **Frontend changes**: Use generated types from Kiota client
3. **Testing**: Test authentication flow first, then video features  
4. **Git workflow**: Use conventional commits (feat, fix, refactor, etc.)

## Resources

- Backend Setup: `PROJECT_SETUP.md`
- Frontend Setup: `FRONTEND_SETUP.md`
- Backend Copilot Instructions: `.github/copilot-instructions.md`
- Frontend Copilot Instructions: `video-manager-frontend/.github/copilot-instructions.md`

## Support

For issues or questions:
1. Check the setup documentation
2. Review Copilot instruction files
3. Consult official documentation for each technology
4. Check console/terminal for error messages

---

**Status**: Frontend scaffolding complete ✅ | Backend setup documented ✅ | Ready for development 🚀
