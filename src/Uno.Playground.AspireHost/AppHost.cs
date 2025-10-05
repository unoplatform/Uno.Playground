var builder = DistributedApplication.CreateBuilder(args);

var azurite = builder
	.AddContainer("azurite", "mcr.microsoft.com/azure-storage/azurite")
	.WithHttpEndpoint(port: 10000, name: "blobstorage", targetPort: 10000);

// API Project
// Expose the API on a fixed host port so the playground can reliably call it.
const int apiHostPort = 54050;

var api = builder.AddProject<Projects.Uno_Playground_Api>("api")
	.WithReference(azurite.GetEndpoint("blobstorage"))
	.WithHttpEndpoint(port: apiHostPort, name: "api")
	.WaitFor(azurite);

// Web Project (Playground)
var playground = builder.AddProject<Projects.Uno_Playground_WASM>("playground")
	.WithReference(api.GetEndpoint("api"))
	.WithExternalHttpEndpoints()
	.WaitFor(api);

builder.Build().Run();
