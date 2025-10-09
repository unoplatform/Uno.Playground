using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Uno.Playground.Api.Models;

namespace Uno.Playground.SamplePump;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, IHostApplicationLifetime hostApplicationLifetime)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SamplePump Worker running at: {time}", DateTimeOffset.Now);

        try
        {
            var publicClient = _httpClientFactory.CreateClient("PublicApi");
            var localClient = _httpClientFactory.CreateClient("LocalApi");

            var categories = await publicClient.GetFromJsonAsync<SampleCategoryViewModel[]>("samples", stoppingToken);
            if (categories == null)
            {
                _logger.LogError("Failed to fetch samples from public API");
                return;
            }

            foreach (var category in categories)
            {
                foreach (var sample in category.Samples)
                {
                    var detail = await publicClient.GetFromJsonAsync<SampleDetailViewModel>($"samples/{sample.Id}", stoppingToken);
                    if (detail != null)
                    {
                        var saveRequest = new SampleSaveRequest
                        {
                            App = "Uno.Playground",
                            Category = detail.Category,
                            Title = detail.Title,
                            Xaml = detail.Xaml,
                            Data = detail.Data,
                            PathData = sample.IconPath,
                            AccentPathData = sample.IconAccentPath
                        };

                        var response = await localClient.PutAsJsonAsync($"samples/{detail.Id}", saveRequest, stoppingToken);
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Pumped sample {Id}: {Title}", detail.Id, detail.Title);
                        }
                        else
                        {
                            _logger.LogError("Failed to save sample {Id}: {Status}", detail.Id, response.StatusCode);
                        }
                    }
                }
            }

            _logger.LogInformation("Sample pumping completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sample pumping");
        }
        finally
        {
            _hostApplicationLifetime.StopApplication();
        }
    }
}