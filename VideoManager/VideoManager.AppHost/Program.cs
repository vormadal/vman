using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .AddDatabase("videomanager");

var apiService = builder.AddProject<Projects.VManBackend>("apiservice")
    .WithReference(postgres)
    .WaitFor(postgres);

// Add Next.js frontend
var frontend = builder.AddJavaScriptApp("frontend", "../../video-manager-frontend", "dev")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Build().Run();
