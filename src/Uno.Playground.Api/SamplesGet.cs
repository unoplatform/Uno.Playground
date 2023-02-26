using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Uno.UI.Demo.Api.Models;

namespace Uno.UI.Demo.Api;

public class SamplesGet
{
	private readonly ILogger _logger;

	public SamplesGet(ILoggerFactory loggerFactory)
		=> _logger = loggerFactory.CreateLogger<SampleGet>();

	[Function("SamplesGet")]
	public async Task<HttpResponseData> Run(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "samples")]
		HttpRequestData req)
	{
		string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
		var tableClient = new TableClient(connectionString, Constants.SamplesTableName);

		var categories = await tableClient
			.QueryAsync<SampleCategory>(cat => cat.PartitionKey.Equals(nameof(SampleCategory)) && !cat.RowKey.Equals(Constants.DefaultCategoryIdForSaving))
			.OrderBy(c => c.ListingOrder)
			.ThenBy(c=>c.Title)
			.ThenBy(c => c.Id)
			.ToArrayAsync();

		var result = new List<SampleCategoryViewModel>(categories.Length);

		var etagSb = new StringBuilder();

		etagSb.Append('"');

		foreach (var category in categories)
		{
			var samples = await tableClient
				.QueryAsync<Sample>(smpl => smpl.PartitionKey.Equals(nameof(Sample)) && smpl.Category.Equals(category.Id))
				.OrderBy(cat => cat.ListingOrder)
				.ThenBy(cat => cat.Title)
				.ThenBy(cat => cat.Id)
				.ToArrayAsync();

			var categoryVm = new SampleCategoryViewModel(category, samples);
			result.Add(categoryVm);
			etagSb.Append(categoryVm.SamplesHash.ToString("X"));
		}

		etagSb.Append('"');

		var etag = etagSb.ToString();

		if (req.Headers.TryGetValues("If-Match", out var matches) && matches.Any(im => im.Equals(etag, StringComparison.OrdinalIgnoreCase)))
		{
			return req.CreateResponse(HttpStatusCode.NotModified);
		}

		var response = req.CreateResponse(HttpStatusCode.OK);
		response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
		response.Headers.Add("ETag", new EntityTagHeaderValue(etag, false).ToString());

		var caceCacheControl = new CacheControlHeaderValue
		{
			NoCache = false,
			Private = false,
			MaxAge = TimeSpan.FromHours(0.5),
			MustRevalidate = false,
			NoStore = false,
			NoTransform = true,
			MaxStale = true
		};

		response.Headers.Add("CacheControl", caceCacheControl.ToString());

		response.WriteString(JsonSerializer.Serialize(result));

		return response;
	}
}
