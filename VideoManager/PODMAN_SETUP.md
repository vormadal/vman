# Using Podman with .NET Aspire

.NET Aspire supports Podman as a container runtime alternative to Docker.

## Prerequisites

1. **Install Podman Desktop** or **Podman CLI**
   - Download from: https://podman.io/getting-started/installation

2. **Enable Podman Machine** (Windows/macOS)
   ```powershell
   podman machine init
   podman machine start
   ```

3. **Verify Podman is Running**
   ```powershell
   podman ps
   ```

## Configuring Aspire to Use Podman

### Option 1: Environment Variable (Recommended)

Set the container runtime before running AppHost:

```powershell
$env:ASPIRE_CONTAINER_RUNTIME="podman"
cd VideoManager.AppHost
dotnet run
```

Or set it globally in your system environment variables:
- Variable: `ASPIRE_CONTAINER_RUNTIME`
- Value: `podman`

### Option 2: Docker Compatibility Mode

Podman can emulate Docker's socket, allowing Aspire to detect it automatically:

**Windows (PowerShell as Administrator):**
```powershell
podman machine set --rootful
podman machine stop
podman machine start
```

Then enable Docker compatibility:
```powershell
# Podman will listen on Docker's default socket
podman system service --time=0
```

### Option 3: AppHost Configuration

Modify `VideoManager.AppHost/Program.cs` to explicitly configure the container runtime:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Set container runtime
builder.Configuration["ASPIRE_CONTAINER_RUNTIME"] = "podman";

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("videomanager");

var api = builder.AddProject<Projects.VideoManager>("api")
    .WithReference(postgres);

builder.Build().Run();
```

## Verification

After starting the AppHost, verify containers are running:

```powershell
podman ps
```

You should see:
- PostgreSQL container
- pgAdmin container (if included)

## Troubleshooting

### "Cannot connect to Podman"
- Ensure Podman machine is running: `podman machine start`
- Check Podman socket: `podman system connection list`

### Port Conflicts
- Podman uses different default ports than Docker
- Check Aspire dashboard for actual assigned ports

### Rootless vs Rootful
- Podman runs rootless by default (more secure)
- Some scenarios may require rootful mode: `podman machine set --rootful`

### Volume Mounts
- Podman handles volume mounts differently on Windows
- Ensure Podman machine has access to necessary directories

## Advantages of Podman

✅ **Rootless by default** - Better security
✅ **No daemon** - Lighter resource usage
✅ **Docker-compatible** - Same CLI commands
✅ **Pod support** - Native Kubernetes-like pods

## Running the Application

Once configured, run as normal:

```powershell
$env:ASPIRE_CONTAINER_RUNTIME="podman"
cd VideoManager.AppHost
dotnet run
```

Or if using Docker compatibility mode:

```powershell
cd VideoManager.AppHost
dotnet run
```

The Aspire dashboard will show all containers managed by Podman.
