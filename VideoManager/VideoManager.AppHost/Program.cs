using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithEndpoint(port: 5432, targetPort: 5432, name: "postgres")
    .AddDatabase("videomanager");

var apiService = builder.AddProject<Projects.VManBackend>("apiservice", launchProfileName: "https")
    .WithReference(postgres)
    .WaitFor(postgres);

// Add Next.js frontend
var frontend = builder.AddJavaScriptApp("frontend", "../../video-manager-frontend", "dev")
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
