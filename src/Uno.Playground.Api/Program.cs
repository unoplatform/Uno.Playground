using System;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Uno.Playground.Api;

public class Program
{
	public static void Main(string[] args)
	{
		var host = Host.CreateDefaultBuilder(args)
			.ConfigureAppConfiguration((context, builder) =>
			{
				builder.AddEnvironmentVariables();
			})
			.ConfigureServices((context, services) =>
			{
				var configuration = context.Configuration;
				var storageConn = configuration["AzureWebJobsStorage"] ?? configuration["ConnectionStrings:AzureWebJobsStorage"] ?? Environment.GetEnvironmentVariable("AzureWebJobsStorage");
				if (string.IsNullOrWhiteSpace(storageConn))
				{
					throw new InvalidOperationException("AzureWebJobsStorage connection string is required in configuration");
				}

				// Register TableServiceClient as singleton
				services.AddSingleton(sp => new TableServiceClient(storageConn));

				// Register a factory to get TableClient instances by table name
				services.AddSingleton<Func<string, TableClient>>(sp => tableName =>
				{
					var svc = sp.GetRequiredService<TableServiceClient>();
					return svc.GetTableClient(tableName);
				});
			})
			.ConfigureFunctionsWorkerDefaults()
			.Build();

		host.Run();
	}
}