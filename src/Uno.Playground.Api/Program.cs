using System;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello");

var host = new HostBuilder()
	.ConfigureFunctionsWorkerDefaults()
	.Build();

host.Run();
