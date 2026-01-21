# OpenAPI Configuration

This document describes the OpenAPI setup for the VManBackend API.

## Current Status

✅ **OpenAPI is configured and available**
- OpenAPI Version: **3.1.1**
- API Title: **VManBackend | v1**
- API Version: **1.0.0**
- Endpoint: `https://localhost:7213/openapi/v1.json`

⚠️ **No API endpoints are currently documented** (empty `paths` object)

## Configuration

The backend uses .NET 10's built-in OpenAPI support (not Swashbuckle/Swagger).

### In `Program.cs`:

```csharp
// Add OpenAPI services
builder.Services.AddOpenApi();

// Map OpenAPI endpoint (only in Development)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
```

## Accessing the OpenAPI Spec

### Direct JSON Download
```bash
curl -k https://localhost:7213/openapi/v1.json
```

### Current Response
```json
{
  "openapi": "3.1.1",
  "info": {
    "title": "VManBackend | v1",
    "version": "1.0.0"
  },
  "servers": [
    {
      "url": "https://localhost:7213/"
    }
  ],
  "paths": {}
}
```

## Why No Paths?

The empty `paths` object indicates that no API endpoints are currently registered or documented. Possible reasons:

1. **Controllers not yet implemented** - The backend infrastructure is set up but features haven't been added
2. **Endpoints not mapped** - Controllers exist but aren't being mapped with `app.MapControllers()`
3. **Minimal API endpoints** - Using minimal API style instead of controllers

### Current Controller Setup
```csharp
builder.Services.AddControllers();  // Controllers are registered
app.MapControllers();               // Controllers are mapped
```

Controllers should be automatically discovered and documented when created.

## Adding Swagger UI (Optional)

While the backend uses built-in OpenAPI, you can add Swagger UI for better visualization:

### 1. Install Package
```bash
dotnet add package Swashbuckle.AspNetCore
```

### 2. Configure in Program.cs
```csharp
// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi(); // Keep both for compatibility
}
```

### 3. Access Swagger UI
Navigate to: `https://localhost:7213/swagger`

## Integration with Frontend (Kiota)

The frontend uses Kiota to generate TypeScript API clients from the OpenAPI spec.

### Kiota Configuration (`video-manager-frontend/kiota-config.json`)
```json
{
  "openapi": "https://localhost:7213/openapi/v1.json",
  "language": "typescript",
  "output": "./src/lib/api/generated",
  "clientClassName": "VideoManagerClient"
}
```

### Generate Client
```bash
cd video-manager-frontend
kiota generate -c kiota-config.json
```

## Troubleshooting

### OpenAPI Endpoint Not Found
- Verify you're in Development environment
- Check that `.MapOpenApi()` is called in `Program.cs`
- Ensure API is running via Aspire AppHost

### Empty Paths Object
- Add controllers in `Features/` directory following vertical slice pattern
- Ensure controllers are decorated with `[ApiController]` and `[Route]` attributes
- Verify `app.MapControllers()` is called in `Program.cs`

### SSL/Certificate Issues
- Use `-k` or `--insecure` flag with curl for self-signed certificates
- Run Aspire to automatically trust development certificates

## Next Steps

To populate the OpenAPI spec with endpoints:

1. **Create feature controllers** in `Features/` directory
2. **Add API endpoints** using controller actions or minimal APIs
3. **Document endpoints** with XML comments or attributes
4. **Regenerate Kiota client** in frontend after adding endpoints

## References

- [ASP.NET Core OpenAPI Documentation](https://learn.microsoft.com/aspnet/core/fundamentals/openapi)
- [Kiota Documentation](https://learn.microsoft.com/openapi/kiota/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
