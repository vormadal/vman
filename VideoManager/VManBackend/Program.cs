using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VManBackend.Common.Data;
using VManBackend.Infrastructure.Authentication;
using VManBackend.Infrastructure.Immich;
using VManBackend.Mediator;
using VManBackend.Features.Assets;
using VManBackend.Features.Authentication;
using VideoManager.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Database
builder.AddNpgsqlDbContext<ApplicationDbContext>("videomanager");

// Add JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "VManBackend",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "VManBackend",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add Services
builder.Services.AddScoped<IJwtService, JwtService>();

// Add Mediator and Handlers
builder.Services.AddMediator();
builder.Services.AddRequestHandler<GetAssets.Handler, GetAssets.Request, GetAssets.Response>();
builder.Services.AddRequestHandler<GetAssetById.Handler, GetAssetById.Request, GetAssetById.Response?>();
builder.Services.AddRequestHandler<GetAssetStatistics.Handler, GetAssetStatistics.Request, GetAssetStatistics.Response>();
builder.Services.AddRequestHandler<Register.Handler, Register.Request, Register.Response?>();
builder.Services.AddRequestHandler<Login.Handler, Login.Request, Login.Response?>();

// Configure JSON options for minimal APIs
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Add Controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
});

// Add Immich client
builder.Services.AddImmichClient(options =>
{
    options.BaseUrl = builder.Configuration["Immich:BaseUrl"] ?? "http://localhost:2283/api";
    options.ApiKey = Environment.GetEnvironmentVariable("IMMICH_API_KEY") ?? throw new InvalidOperationException("IMMICH_API_KEY environment variable is required");
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Authentication Endpoints (no authorization required)
var authGroup = app.MapGroup("/api/auth");

authGroup.MapPost("/register", async (Register.Request request, IMediator mediator) =>
{
    if (!Register.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    var response = await mediator.Send(request);
    return response != null 
        ? Results.Ok(response) 
        : Results.Conflict(new { error = "Email already in use" });
})
.WithName("Register")
.WithOpenApi();

authGroup.MapPost("/login", async (Login.Request request, IMediator mediator) =>
{
    if (!Login.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    var response = await mediator.Send(request);
    return response != null 
        ? Results.Ok(response) 
        : Results.Unauthorized();
})
.WithName("Login")
.WithOpenApi();

// Asset Endpoints
var assetsGroup = app.MapGroup("/api/assets")
    .RequireAuthorization();

assetsGroup.MapGet("/", async (
    IMediator mediator,
    int? assetType,
    int page = 1,
    int pageSize = 50,
    string? sortBy = "CreatedAt",
    bool descending = true) =>
{
    var request = new GetAssets.Request(
        assetType.HasValue ? (VManBackend.Common.Models.AssetType)assetType.Value : null,
        page,
        pageSize,
        sortBy,
        descending
    );

    if (!GetAssets.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    var response = await mediator.Send(request);
    return Results.Ok(response);
})
.WithName("GetAssets")
.WithOpenApi();

assetsGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var request = new GetAssetById.Request(id);
    var response = await mediator.Send(request);
    return response != null ? Results.Ok(response) : Results.NotFound();
})
.WithName("GetAssetById")
.WithOpenApi();

assetsGroup.MapGet("/statistics", async (IMediator mediator) =>
{
    var request = new GetAssetStatistics.Request();
    var response = await mediator.Send(request);
    return Results.Ok(response);
})
.WithName("GetAssetStatistics")
.WithOpenApi();

app.MapDefaultEndpoints();

// Apply migrations on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
