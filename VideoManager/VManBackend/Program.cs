using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VManBackend.Common.Data;
using VManBackend.Infrastructure.Authentication;
using VManBackend.Infrastructure.Immich;
using VManBackend.Infrastructure.Providers;
using VManBackend.Mediator;
// using VManBackend.Features.Assets; // Temporarily disabled
using VManBackend.Features.Authentication;
using VManBackend.Features.Tags;
using VManBackend.Features.Items;
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

// Add Media Providers
builder.Services.AddScoped<VManBackend.Infrastructure.Providers.IMediaProvider, VManBackend.Infrastructure.Providers.ImmichMediaProvider>();

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

// Tag Endpoints (require authorization)
var tagsGroup = app.MapGroup("/api/tags").RequireAuthorization();

tagsGroup.MapGet("/", async (IMediator mediator, string? search = null, int page = 1, int pageSize = 50) =>
{
    var request = new GetTags.Request(search, page, pageSize);
    if (!GetTags.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    var response = await mediator.Send(request);
    return Results.Ok(response);
})
.WithName("GetTags")
.WithOpenApi();

tagsGroup.MapGet("/{id:guid}", async (IMediator mediator, Guid id) =>
{
    var request = new GetTagById.Request(id);
    if (!GetTagById.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    try
    {
        var response = await mediator.Send(request);
        return Results.Ok(response);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
.WithName("GetTagById")
.WithOpenApi();

tagsGroup.MapPost("/", async (IMediator mediator, CreateTag.Request request) =>
{
    if (!CreateTag.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    try
    {
        var response = await mediator.Send(request);
        return Results.Created($"/api/tags/{response.Id}", response);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
})
.WithName("CreateTag")
.WithOpenApi();

tagsGroup.MapPut("/{id:guid}", async (IMediator mediator, Guid id, RenameTag.Request request) =>
{
    // Ensure the ID in the route matches the request
    if (id != request.Id)
    {
        return Results.BadRequest(new { error = "ID mismatch" });
    }

    if (!RenameTag.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    try
    {
        var response = await mediator.Send(request);
        return Results.Ok(response);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
.WithName("RenameTag")
.WithOpenApi();

tagsGroup.MapDelete("/{id:guid}", async (IMediator mediator, Guid id) =>
{
    var request = new DeleteTag.Request(id);
    if (!DeleteTag.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    try
    {
        var response = await mediator.Send(request);
        return Results.Ok(response);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
.WithName("DeleteTag")
.WithOpenApi();

// Item Endpoints (require authorization)
var itemsGroup = app.MapGroup("/api/items").RequireAuthorization();

itemsGroup.MapGet("/", async (IMediator mediator, string? provider = null, string? type = null, bool? isFavorite = null, string? sortBy = "createdAt", bool sortDescending = true, int page = 1, int pageSize = 50) =>
{
    // Parse type if provided
    MediaType? mediaType = null;
    if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<MediaType>(type, true, out var parsedType))
    {
        mediaType = parsedType;
    }

    var request = new GetItems.Request(provider, mediaType, isFavorite, sortBy, sortDescending, page, pageSize);
    if (!GetItems.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    var response = await mediator.Send(request);
    return Results.Ok(response);
})
.WithName("GetItems")
.WithOpenApi();

itemsGroup.MapGet("/{provider}/{id}", async (IMediator mediator, string provider, string id) =>
{
    var request = new GetItemById.Request(provider, id);
    if (!GetItemById.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    var response = await mediator.Send(request);
    return response != null 
        ? Results.Ok(response) 
        : Results.NotFound(new { error = "Item not found" });
})
.WithName("GetItemById")
.WithOpenApi();

itemsGroup.MapPost("/{provider}/{id}/tags", async (IMediator mediator, string provider, string id, AddTagToItem.Request request) =>
{
    // Ensure provider and id from route match request
    var fullRequest = new AddTagToItem.Request(provider, id, request.TagId);
    
    if (!AddTagToItem.Validator.Validate(fullRequest, out var error))
    {
        return Results.BadRequest(new { error });
    }

    try
    {
        var response = await mediator.Send(fullRequest);
        return Results.Ok(response);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
.WithName("AddTagToItem")
.WithOpenApi();

itemsGroup.MapDelete("/{provider}/{id}/tags/{tagId:guid}", async (IMediator mediator, string provider, string id, Guid tagId) =>
{
    var request = new RemoveTagFromItem.Request(provider, id, tagId);
    
    if (!RemoveTagFromItem.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    var response = await mediator.Send(request);
    return Results.Ok(response);
})
.WithName("RemoveTagFromItem")
.WithOpenApi();

// Tag-based item queries
tagsGroup.MapGet("/{id:guid}/items", async (IMediator mediator, Guid id, int page = 1, int pageSize = 50) =>
{
    var request = new GetItemsByTag.Request(id, page, pageSize);
    
    if (!GetItemsByTag.Validator.Validate(request, out var error))
    {
        return Results.BadRequest(new { error });
    }

    try
    {
        var response = await mediator.Send(request);
        return Results.Ok(response);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
.WithName("GetItemsByTag")
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
