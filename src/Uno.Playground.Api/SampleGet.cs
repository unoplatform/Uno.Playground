using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Uno.Playground.Api.Helpers;
using Uno.Playground.Api.Models;

namespace Uno.Playground.Api;

public static class SampleGet
{
	[Function("SampleGet")]
	public static async Task<HttpResponseData> Run(
		[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "samples/{id}")] HttpRequestData req,
		string id,
		FunctionContext context)
	{
		var logger = context.GetLogger("SampleGet");
		var tableService = context.InstanceServices.GetService(typeof(TableServiceClient)) as TableServiceClient;
		if (tableService == null)
		{
			var err = req.CreateResponse();
			err.StatusCode = System.Net.HttpStatusCode.InternalServerError;
			await err.WriteStringAsync("TableServiceClient not configured.");
			return err;
		}
		var table = tableService.GetSamplesTableClient();

		var response = req.CreateResponse();

		// Query the table for the specific RowKey using a filter string
		var query = table.QueryAsync<Sample>(filter: $"PartitionKey eq '{nameof(Sample)}' and RowKey eq '{id}'");
		await foreach (var item in query)
		{
			var vm = new SampleDetailViewModel(item);
			response.StatusCode = System.Net.HttpStatusCode.OK;
			await response.WriteAsJsonAsync(vm);
			return response;
		}

		response.StatusCode = System.Net.HttpStatusCode.NotFound;
		await response.WriteStringAsync("Sample not found");
		return response;
	}
}