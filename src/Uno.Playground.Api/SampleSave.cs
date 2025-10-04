using System.Threading.Tasks;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Uno.Playground.Api.Helpers;
using Uno.Playground.Api.Models;

namespace Uno.Playground.Api;

public static class SampleSave
{
	[Function("SampleSave")]
	public static async Task<HttpResponseData> Run(
		[HttpTrigger(AuthorizationLevel.Admin, "put", Route = "samples/{id}")] HttpRequestData req,
		string id,
		FunctionContext context)
	{
		var logger = context.GetLogger("SampleSave");
		var tableService = context.InstanceServices.GetService(typeof(TableServiceClient)) as TableServiceClient;
		if (tableService == null)
		{
			var err = req.CreateResponse();
			err.StatusCode = System.Net.HttpStatusCode.InternalServerError;
			await err.WriteStringAsync("TableServiceClient not configured.");
			return err;
		}
		var table = tableService.GetSamplesTableClient();

		var body = await req.ReadAsStringAsync();

		if (string.IsNullOrWhiteSpace(body))
		{
			var resp = req.CreateResponse();
			resp.StatusCode = System.Net.HttpStatusCode.BadRequest;
			await resp.WriteStringAsync("Request payload required.");
			return resp;
		}

		var jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		var saveRequest = JsonSerializer.Deserialize<SampleSaveRequest?>(body, jsonOptions);

		var response = req.CreateResponse();

	// Try to get existing entity
	Sample? existing = null;
		var existingQuery = table.QueryAsync<Sample>(filter: $"PartitionKey eq '{nameof(Sample)}' and RowKey eq '{id}'");
		await foreach (var e in existingQuery)
		{
			existing = e;
			break;
		}

		var sample = existing ?? new Sample(id);
		var exists = existing != null;

		if (saveRequest == null)
		{
			response.StatusCode = System.Net.HttpStatusCode.BadRequest;
			await response.WriteStringAsync("Request payload required.");
			return response;
		}

		if (!exists && (saveRequest.Xaml == null || saveRequest.Xaml.Length < 5))
		{
			response.StatusCode = System.Net.HttpStatusCode.BadRequest;
			await response.WriteStringAsync("Xaml required.");
			return response;
		}

		if (saveRequest.Xaml != null && saveRequest.Xaml.Length > 512 * 1024)
		{
			response.StatusCode = System.Net.HttpStatusCode.RequestEntityTooLarge;
			await response.WriteStringAsync("Xaml too big.");
			return response;
		}

		if (saveRequest.Data != null && saveRequest.Data.Length > 16 * 1024)
		{
			response.StatusCode = System.Net.HttpStatusCode.RequestEntityTooLarge;
			await response.WriteStringAsync("Data too big.");
			return response;
		}

		if (saveRequest.Title != null && saveRequest.Title.Length > 255)
		{
			response.StatusCode = System.Net.HttpStatusCode.RequestEntityTooLarge;
			await response.WriteStringAsync("Title too big.");
			return response;
		}

		if (saveRequest.App != null && saveRequest.App.Length > 255)
		{
			response.StatusCode = System.Net.HttpStatusCode.RequestEntityTooLarge;
			await response.WriteStringAsync("App name too big.");
			return response;
		}

		if (!string.IsNullOrWhiteSpace(saveRequest.Data))
		{
			sample.Data = saveRequest.Data;
		}

		if (!string.IsNullOrWhiteSpace(saveRequest.Xaml))
		{
			sample.Xaml = saveRequest.Xaml;
		}

		if (string.IsNullOrWhiteSpace(sample.Category) || !string.IsNullOrWhiteSpace(saveRequest.Category))
		{
			sample.Category = saveRequest.Category ?? Constants.DefaultCategoryIdForSaving;
		}
		if (!string.IsNullOrWhiteSpace(saveRequest.Title))
		{
			sample.Title = saveRequest.Title;
		}
		if (string.IsNullOrWhiteSpace(sample.IpAddress))
		{
			sample.IpAddress = req.GetClientIp();
		}
		if (string.IsNullOrWhiteSpace(sample.UserAgent))
		{
			sample.UserAgent = (req.Headers.TryGetValues("User-Agent", out var ua) ? string.Join(" ", ua) : null);
		}
		if (!string.IsNullOrWhiteSpace(saveRequest.App))
		{
			sample.App = saveRequest.App;
		}
		if (!string.IsNullOrWhiteSpace(saveRequest.PathData))
		{
			sample.PathData = saveRequest.PathData;
		}
		if (!string.IsNullOrWhiteSpace(saveRequest.AccentPathData))
		{
			sample.AccentPathData = saveRequest.AccentPathData;
		}

		// Upsert the entity
		await table.UpsertEntityAsync(sample, TableUpdateMode.Merge);

		response.StatusCode = System.Net.HttpStatusCode.OK;
		await response.WriteStringAsync(id);
		return response;
	}
}