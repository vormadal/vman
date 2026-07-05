# Dockerfile for Caprover Deployment

## Goal
Create a single Dockerfile that builds and runs both the Next.js frontend and .NET backend as one deployable unit for Caprover.

## Approach
1. Multi-stage Dockerfile that:
   - Builds the .NET backend (VManBackend)
   - Builds the Next.js frontend (optimized production build)
   - Combines both into a final runtime image
   - Backend serves the static frontend files and API endpoints

2. Configuration:
   - Backend will serve both API routes and static frontend files
   - Next.js standalone output for minimal production deployment
   - PostgreSQL connection via environment variables
   - JWT and other secrets via environment variables

## Architecture
- Frontend: Next.js standalone build (self-contained)
- Backend: ASP.NET Core serving both API and static files
- Runtime: .NET backend as the main process, serving frontend from wwwroot or static folder

## Implementation Steps
1. Update Next.js config for standalone output
2. Modify backend to serve static files from frontend build
3. Create unified Dockerfile in repository root
4. Test the build locally

## Required Production Environment Variables

See `.env.example` at the repository root for the full list (`Jwt__SecretKey`,
`ConnectionStrings__videomanager`, `IMMICH_API_KEY`/`IMMICH_ADMIN_PASSWORD`).
None of these have real-looking defaults in `appsettings.json` -- the app
throws a startup error if `Jwt:SecretKey` is missing.
