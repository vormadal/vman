using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VManBackend.Common.Data;
using VManBackend.Infrastructure.Authentication;
using VManBackend.Infrastructure.Immich;
using VManBackend.Infrastructure.Providers;
using VManBackend.Infrastructure.Data;
using VManBackend.Mediator;
using VManBackend.Endpoints;
// using VManBackend.Features.Assets; // Temporarily disabled
using VManBackend.Features.Authentication;
using VManBackend.Features.Tags;
using VManBackend.Features.Items;
using VManBackend.Features.Sync;
using VManBackend.Infrastructure.Sync;
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

// Add CORS - Allow HTTP for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Services
builder.Services.AddScoped<IJwtService, JwtService>();

// Add Mediator and Handlers
builder.Services.AddMediator();
// Asset handlers - disabled
// builder.Services.AddRequestHandler<GetAssets.Handler, GetAssets.Request, GetAssets.Response>();
// builder.Services.AddRequestHandler<GetAssetById.Handler, GetAssetById.Request, GetAssetById.Response?>();
// builder.Services.AddRequestHandler<GetAssetStatistics.Handler, GetAssetStatistics.Request, GetAssetStatistics.Response>();

// Authentication handlers
builder.Services.AddRequestHandler<Register.Handler, Register.Request, Register.Response?>();
builder.Services.AddRequestHandler<Login.Handler, Login.Request, Login.Response?>();

// Tag handlers
builder.Services.AddRequestHandler<CreateTag.Handler, CreateTag.Request, CreateTag.Response>();
builder.Services.AddRequestHandler<RenameTag.Handler, RenameTag.Request, RenameTag.Response>();
builder.Services.AddRequestHandler<DeleteTag.Handler, DeleteTag.Request, DeleteTag.Response>();
builder.Services.AddRequestHandler<GetTags.Handler, GetTags.Request, GetTags.Response>();
builder.Services.AddRequestHandler<GetTagById.Handler, GetTagById.Request, GetTagById.Response>();

// Item handlers
builder.Services.AddRequestHandler<AddTagToItem.Handler, AddTagToItem.Request, AddTagToItem.Response>();
builder.Services.AddRequestHandler<RemoveTagFromItem.Handler, RemoveTagFromItem.Request, RemoveTagFromItem.Response>();
builder.Services.AddRequestHandler<GetItemsByTag.Handler, GetItemsByTag.Request, GetItemsByTag.Response>();
builder.Services.AddRequestHandler<GetItems.Handler, GetItems.Request, GetItems.Response>();
builder.Services.AddRequestHandler<GetItemById.Handler, GetItemById.Request, GetItemById.Response?>();

// Sync handlers
builder.Services.AddRequestHandler<TriggerSync.Handler, TriggerSync.Request, TriggerSync.Response?>();
builder.Services.AddRequestHandler<GetSyncStatus.Handler, GetSyncStatus.Request, GetSyncStatus.Response?>();
builder.Services.AddRequestHandler<CancelSync.Handler, CancelSync.Request, CancelSync.Response?>();

// Background sync infrastructure
builder.Services.AddSingleton<SyncChannel>();
builder.Services.AddHostedService<SyncBackgroundService>();
builder.Services.AddScoped<ImmichSyncProcessor>();

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

// Add ProblemDetails support
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
});

// Add Immich client
builder.Services.AddImmichClient(options =>
{
    options.BaseUrl = builder.Configuration["Immich:BaseUrl"] ?? throw new InvalidOperationException("Immich BaseUrl is not configured");
    options.ApiKey = Environment.GetEnvironmentVariable("IMMICH_API_KEY") ?? throw new InvalidOperationException("IMMICH_API_KEY environment variable is required");
});

// Add Media Providers
builder.Services.AddMemoryCache(); // For caching provider responses
builder.Services.AddScoped<ImmichMediaProvider>(); // Register concrete provider
builder.Services.AddScoped<IMediaProvider>(sp =>
{
    var immichProvider = sp.GetRequiredService<ImmichMediaProvider>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    return new CachedMediaProvider(immichProvider, cache); // Wrap with caching
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Apply middleware
app.UseCors();

// Add ProblemDetails middleware for exception handling
app.UseExceptionHandler();
app.UseStatusCodePages();

// For production, HTTPS redirection is handled by reverse proxy/hosting
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map API Endpoints
app.MapAuthEndpoints();
app.MapTagEndpoints();
app.MapItemEndpoints();
app.MapSyncEndpoints();
app.MapProviderEndpoints();

// Apply migrations and seed data on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    
    db.Database.Migrate();
    await DbSeeder.SeedTestUserAsync(db, config);
}

app.Run();
