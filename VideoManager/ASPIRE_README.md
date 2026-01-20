# .NET Aspire Configuration

This project uses .NET Aspire for cloud-native development, providing orchestration for PostgreSQL and other services during development.

## Project Structure

```
VideoManager/
├── VideoManager.AppHost/          # Aspire orchestration project
│   └── Program.cs                 # Defines service composition
├── VideoManager.ServiceDefaults/  # Shared configuration library
│   └── Extensions.cs              # OpenTelemetry, health checks, resilience
└── VideoManager/                  # Main API project
    └── Program.cs                 # Uses Aspire service defaults
```

## Running the Application

### Option 1: Run with Aspire (Recommended for Development)

Start the Aspire AppHost which will automatically start PostgreSQL and the API:

```powershell
cd VideoManager.AppHost
dotnet run
```

This will:
- Start PostgreSQL in a container with pgAdmin
- Start the VideoManager API
- Open the Aspire dashboard showing all services, logs, traces, and metrics

The dashboard URL will be displayed in the console (usually `http://localhost:15000`).

### Option 2: Run API Standalone

If you prefer to run just the API with an existing PostgreSQL instance:

```powershell
cd VideoManager
dotnet run
```

## What Aspire Provides

### 1. **Service Orchestration**
- Automatically starts PostgreSQL container
- Includes pgAdmin for database management
- Manages service dependencies and startup order

### 2. **Observability**
- **OpenTelemetry** integration for distributed tracing
- **Metrics** collection (HTTP requests, runtime, EF Core)
- **Structured logging** with correlation IDs
- **Aspire Dashboard** for real-time monitoring

### 3. **Resilience**
- HTTP client resilience patterns (retry, circuit breaker, timeout)
- Service discovery for inter-service communication
- Health checks for all services

### 4. **Service Defaults**
All projects using `builder.AddServiceDefaults()` get:
- OpenTelemetry traces and metrics
- HTTP client resilience (automatic retries, timeouts)
- Health check endpoints (`/health`, `/alive`)
- Service discovery integration

## Database Management

The PostgreSQL database is automatically provisioned by Aspire with:
- Database name: `videomanager`
- Connection string automatically injected
- Accessible via pgAdmin at the URL shown in Aspire dashboard

### Running Migrations

```powershell
cd VideoManager
dotnet dotnet-ef database update
```

## Endpoints

When running via Aspire AppHost:
- **API**: https://localhost:7XXX (port assigned dynamically, check dashboard)
- **Aspire Dashboard**: http://localhost:15XXX
- **pgAdmin**: http://localhost:XXXX (check dashboard for exact port)

### Health Checks
- `/health` - Overall health status
- `/alive` - Liveness probe

## Configuration

### AppHost Configuration
Edit `VideoManager.AppHost/Program.cs` to:
- Add new services
- Configure service dependencies
- Add volumes or environment variables

### Service Defaults
Edit `VideoManager.ServiceDefaults/Extensions.cs` to:
- Customize OpenTelemetry configuration
- Add additional health checks
- Configure resilience policies

## Development Workflow

1. **Start Aspire**: `dotnet run` in AppHost project
2. **Open Dashboard**: Navigate to the URL shown in console
3. **View Logs**: Real-time logs from all services
4. **Inspect Traces**: Distributed tracing for requests
5. **Monitor Metrics**: HTTP, runtime, and custom metrics

## Environment Variables

Set these before running:
- `IMMICH_API_KEY` - Required for Immich integration

## Aspire Dashboard Features

- **Resources Tab**: See all running services and their status
- **Console Logs**: View stdout/stderr from each service
- **Structured Logs**: Browse structured logs with filtering
- **Traces**: Distributed tracing across services
- **Metrics**: Real-time metrics and charts

## Troubleshooting

### PostgreSQL Not Starting
- Ensure Docker is running
- Check Aspire dashboard for error messages

### Connection String Issues
- Aspire automatically injects connection strings
- Named connection matches database resource name ("videomanager")
- Check dashboard for actual connection string values

### Port Conflicts
- Aspire assigns dynamic ports to avoid conflicts
- Check dashboard for actual URLs and ports
