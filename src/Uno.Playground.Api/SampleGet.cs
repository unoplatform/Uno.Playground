using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Uno.UI.Demo.Api.Helpers;
using Uno.UI.Demo.Api.Models;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using Microsoft.Azure.WebJobs;
using System;

namespace Uno.UI.Demo.Api;

public class SampleGet
{
	private readonly ILogger _logger;

	public SampleGet(ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger<SampleGet>();
	}

	[Function("SampleGet")]
	public async Task<HttpResponseData> Run(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "samples/{id}")] HttpRequestData req,
		string id
	)
	{
		var tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), Constants.SamplesTableName);

		var sampleQuery = tableClient.QueryAsync<Sample>(smpl => smpl.PartitionKey.Equals(nameof(Sample)) && smpl.RowKey.Equals(id));

		var sample = await sampleQuery
			.Select(s => new SampleDetailViewModel(s))
			.FirstOrDefaultAsync();

		if (sample == null)
		{
			var response = req.CreateResponse(HttpStatusCode.NotFound);
			response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

			response.WriteString("Sample not found");

			return response;
		}
		else
		{
			var response = req.CreateResponse(HttpStatusCode.OK);
			response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

			response.WriteString(JsonSerializer.Serialize(sample));

			return response;
		}
	}
}
