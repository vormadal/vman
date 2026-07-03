
var builder = DistributedApplication.CreateBuilder(args);

// Test user credentials (shared between backend and frontend)
var testUserEmail = builder.Configuration["TestUser:Email"] 
    ?? builder.Configuration["TEST_USER_EMAIL"] 
    ?? string.Empty;
var testUserPassword = builder.Configuration["TestUser:Password"] 
    ?? builder.Configuration["TEST_USER_PASSWORD"] 
    ?? string.Empty;
// If IMMICH_API_KEY is left empty, the backend bootstraps one itself via Immich's admin
// sign-up/login/api-key endpoints (see ImmichBootstrapper), using IMMICH_ADMIN_PASSWORD.
var immichApiKey = builder.Configuration["Immich:ApiKey"]
    ?? builder.Configuration["IMMICH_API_KEY"]
    ?? string.Empty;
var immichAdminPassword = builder.Configuration["Immich:AdminPassword"]
    ?? builder.Configuration["IMMICH_ADMIN_PASSWORD"]
    ?? string.Empty;

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false)
    .WithPgAdmin()
    .WithEndpoint(port: 5432, targetPort: 5432, name: "postgres")
    .AddDatabase("videomanager");

var useStubImmich = builder.Configuration["USE_STUB_IMMICH"] ?? "false";

var apiService = builder.AddProject<Projects.VManBackend>("apiservice", launchProfileName: "http")
    .WithReference(postgres)
    .WithEnvironment("TEST_USER_EMAIL", testUserEmail)
    .WithEnvironment("TEST_USER_PASSWORD", testUserPassword)
    .WithEnvironment("USE_STUB_IMMICH", useStubImmich)
    .WithEnvironment("IMMICH_API_KEY", immichApiKey)
    .WithEnvironment("IMMICH_ADMIN_PASSWORD", immichAdminPassword)
    .WaitFor(postgres);

// Add Next.js frontend
var frontend = builder.AddJavaScriptApp("frontend", "../../video-manager-frontend", "dev")
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithExternalHttpEndpoints()
    .WithEnvironment("TEST_USER_EMAIL", testUserEmail)
    .WithEnvironment("TEST_USER_PASSWORD", testUserPassword)
    .WithReference(apiService);

await builder.Build().RunAsync();
