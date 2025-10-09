using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Uno.Playground.SamplePump;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddServiceDiscovery();
        services.ConfigureHttpClientDefaults(http =>
        {
	        http.AddServiceDiscovery();
        });

        services.AddHttpClient("PublicApi", client =>
        {
            client.BaseAddress = new Uri("https://uno-ui-api.azurewebsites.net/api/");
        });

        services.AddHttpClient("LocalApi", client =>
        {
            client.BaseAddress = new Uri("http://api/api/");
        });

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
