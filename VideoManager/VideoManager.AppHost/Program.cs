using System.Security.Cryptography.X509Certificates;
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Test user credentials (shared between backend and frontend)
var testUserEmail = builder.Configuration["TestUser:Email"] 
    ?? builder.Configuration["TEST_USER_EMAIL"] 
    ?? string.Empty;
var testUserPassword = builder.Configuration["TestUser:Password"] 
    ?? builder.Configuration["TEST_USER_PASSWORD"] 
    ?? string.Empty;
var immichApiKey = builder.AddParameter("immich-api-key", secret: true);

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
    .WaitFor(postgres);

// Add Next.js frontend
var frontend = builder.AddJavaScriptApp("frontend", "../../video-manager-frontend", "dev")
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithExternalHttpEndpoints()
    .WithEnvironment("TEST_USER_EMAIL", testUserEmail)
    .WithEnvironment("TEST_USER_PASSWORD", testUserPassword)
    .WithReference(apiService);

builder.Build().Run();
