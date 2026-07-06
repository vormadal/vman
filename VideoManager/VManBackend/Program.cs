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
using VManBackend.Features.Authentication;
using VManBackend.Features.Admin;
using VManBackend.Features.Tags;
using VManBackend.Features.Items;
using VManBackend.Features.Sync;
using VManBackend.Features.Collections;
using VManBackend.Features.People;
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});

// CORS: development only — in production, everything is same-origin via nginx
if (builder.Environment.IsDevelopment())
{
    var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "http://localhost:3000")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });
}

// Add Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddHttpContextAccessor(); // Required for getting current user in handlers

// Add Mediator and Handlers
builder.Services.AddMediator();

// Authentication handlers
builder.Services.AddRequestHandler<Register.Handler, Register.Request, Register.Response?>();
builder.Services.AddRequestHandler<Login.Handler, Login.Request, Login.Response?>();
builder.Services.AddRequestHandler<AcceptInvite.Handler, AcceptInvite.Request, AcceptInvite.Response?>();
builder.Services.AddRequestHandler<CompleteProfile.Handler, CompleteProfile.Request, CompleteProfile.Response?>();

// Admin handlers
builder.Services.AddRequestHandler<CreateInvite.Handler, CreateInvite.Request, CreateInvite.Response?>();
builder.Services.AddRequestHandler<GetInvites.Handler, GetInvites.Request, GetInvites.Response?>();
builder.Services.AddRequestHandler<GetUsers.Handler, GetUsers.Request, GetUsers.Response?>();
builder.Services.AddRequestHandler<BlockUser.Handler, BlockUser.Request, BlockUser.Response?>();
builder.Services.AddRequestHandler<UnblockUser.Handler, UnblockUser.Request, UnblockUser.Response?>();
builder.Services.AddRequestHandler<ChangeUserRole.Handler, ChangeUserRole.Request, ChangeUserRole.Response?>();

// Tag handlers
builder.Services.AddRequestHandler<CreateTag.Handler, CreateTag.Request, CreateTag.Response>();
builder.Services.AddRequestHandler<RenameTag.Handler, RenameTag.Request, RenameTag.Response>();
builder.Services.AddRequestHandler<DeleteTag.Handler, DeleteTag.Request, DeleteTag.Response>();
builder.Services.AddRequestHandler<GetTags.Handler, GetTags.Request, GetTags.Response>();
builder.Services.AddRequestHandler<GetTagById.Handler, GetTagById.Request, GetTagById.Response>();

// People handlers
builder.Services.AddRequestHandler<GetPeople.Handler, GetPeople.Request, GetPeople.Response>();
builder.Services.AddRequestHandler<GetPersonById.Handler, GetPersonById.Request, GetPersonById.Response?>();

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

// Collection handlers
builder.Services.AddRequestHandler<CreateCollection.Handler, CreateCollection.Request, CreateCollection.Response>();
builder.Services.AddRequestHandler<GetCollections.Handler, GetCollections.Request, GetCollections.Response>();
builder.Services.AddRequestHandler<GetCollectionById.Handler, GetCollectionById.Request, GetCollectionById.Response>();
builder.Services.AddRequestHandler<AddItemToCollection.Handler, AddItemToCollection.Request, AddItemToCollection.Response>();
builder.Services.AddRequestHandler<RemoveItemFromCollection.Handler, RemoveItemFromCollection.Request, RemoveItemFromCollection.Response>();
builder.Services.AddRequestHandler<UpdateCollectionItemOrder.Handler, UpdateCollectionItemOrder.Request, UpdateCollectionItemOrder.Response>();
builder.Services.AddRequestHandler<DeleteCollection.Handler, DeleteCollection.Request, DeleteCollection.Response>();
builder.Services.AddRequestHandler<ExportCollectionToShotcut.Handler, ExportCollectionToShotcut.Request, ExportCollectionToShotcut.Response>();
builder.Services.AddRequestHandler<BulkAddFilteredItemsToCollection.Handler, BulkAddFilteredItemsToCollection.Request, BulkAddFilteredItemsToCollection.Response>();
builder.Services.AddRequestHandler<UpdateCollectionItemNote.Handler, UpdateCollectionItemNote.Request, UpdateCollectionItemNote.Response>();

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
var useStubImmich = Environment.GetEnvironmentVariable("USE_STUB_IMMICH") == "true";
builder.Services.AddImmichClient(options =>
{
    if (!useStubImmich)
    {
        options.BaseUrl = builder.Configuration["Immich:BaseUrl"] ?? throw new InvalidOperationException("Immich BaseUrl is not configured");
        options.ApiKey = Environment.GetEnvironmentVariable("IMMICH_API_KEY") ?? throw new InvalidOperationException("IMMICH_API_KEY environment variable is required");
    }
    else
    {
        // Stub mode: API key and BaseUrl not required
        options.BaseUrl = "http://stub";
        options.ApiKey = "stub-key";
    }
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
if (app.Environment.IsDevelopment())
{
    app.UseCors();
}

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
app.MapAdminEndpoints();
app.MapTagEndpoints();
app.MapItemEndpoints();
app.MapSyncEndpoints();
app.MapProviderEndpoints();
app.MapCollectionEndpoints();
app.MapPeopleEndpoints();

// Frontend is served by the Next.js Node process (via nginx proxy in production)

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

await db.Database.MigrateAsync();
await DbSeeder.SeedAdminUserAsync(db, config);

if (app.Environment.IsDevelopment())
{   
    await DbSeeder.SeedTestUserAsync(db, config);
}

await app.RunAsync();
