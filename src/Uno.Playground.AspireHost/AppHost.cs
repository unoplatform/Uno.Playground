using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage").RunAsEmulator();

var apistorage = storage.AddTables("apistorage");

var api = builder.AddAzureFunctionsProject<Projects.Uno_Playground_Api>("api")
	.WithReference(apistorage)
	.WaitFor(apistorage)
	.WithExternalHttpEndpoints();

// Web Project (Playground)
var playground = builder.AddProject<Projects.Uno_Playground_WASM>("playground")
	.WithReference(api)
	.WaitFor(api)
	.WithExternalHttpEndpoints();

// Sample Pump Worker
builder.AddProject<Projects.Uno_Playground_SamplePump>("samplepump")
	.WithReference(api)
	.WaitFor(api);


var scalar = builder.AddScalarApiReference();
scalar.WithApiReference(api);

builder.Build().Run();
