using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Uno.Playground.Api.Helpers;
using Uno.Playground.Api.Models;

namespace Uno.Playground.Api;

public static class SamplesGet
{
	[Function("SamplesGet")]
	public static async Task<HttpResponseData> Run(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "samples")] HttpRequestData req,
		FunctionContext context)
	{
		var logger = context.GetLogger("SamplesGet");
		var tableService = context.InstanceServices.GetService(typeof(TableServiceClient)) as TableServiceClient;
		if (tableService == null)
		{
			var err = req.CreateResponse();
			err.StatusCode = System.Net.HttpStatusCode.InternalServerError;
			await err.WriteStringAsync("TableServiceClient not configured.");
			return err;
		}
		var table = tableService.GetSamplesTableClient();

		// Get categories (exclude default saving category)
		var categories = new List<SampleCategory>();
		var categoriesQuery = table.QueryAsync<SampleCategory>(filter: $"PartitionKey eq '{nameof(SampleCategory)}' and RowKey ne '{Constants.DefaultCategoryIdForSaving}'");
		await foreach (var c in categoriesQuery)
		{
			categories.Add(c);
		}

		var orderedCats = categories
			.OrderBy(cat => cat.ListingOrder)
			.ThenBy(cat => cat.Title)
			.ThenBy(cat => cat.Id)
			.ToArray();

		var result = new List<SampleCategoryViewModel>(orderedCats.Length);
		var etagSb = new StringBuilder();
		etagSb.Append('"');

		foreach (var category in orderedCats)
		{
			var samples = new List<Sample>();
			var samplesQuery = table.QueryAsync<Sample>(filter: $"PartitionKey eq '{nameof(Sample)}' and Category eq '{category.Id}'");
			await foreach (var s in samplesQuery)
			{
				samples.Add(s);
			}

			var orderedSamples = samples
				.OrderBy(s => s.ListingOrder)
				.ThenBy(s => s.Title)
				.ThenBy(s => s.Id)
				.ToArray();

			var categoryVm = new SampleCategoryViewModel(category, orderedSamples);
			result.Add(categoryVm);
			etagSb.Append(categoryVm.SamplesHash.ToString("X"));
		}

		etagSb.Append('"');
		var etag = etagSb.ToString();

		var response = req.CreateResponse();
		response.Headers.Add("Cache-Control", "public, max-age=1800");
		response.Headers.Add("ETag", etag);
		response.StatusCode = System.Net.HttpStatusCode.OK;
		await response.WriteAsJsonAsync(result.ToArray());
		return response;
	}
}