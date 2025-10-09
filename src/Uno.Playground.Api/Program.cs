using System;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddAzureTableServiceClient("apistorage");

builder.Configuration.AddEnvironmentVariables();

var services = builder.Services;

services.AddServiceDiscovery();

services.AddOpenApi();

//builder.UseFunctionExecutionMiddleware();

builder.Build().Run();
