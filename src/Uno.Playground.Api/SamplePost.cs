using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Uno.Playground.Api.Helpers;
using Uno.Playground.Api.Models;

namespace Uno.Playground.Api;

public static class SamplePost
{
	[Function("SamplePost")]
	public static async Task<HttpResponseData> Run(
		[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "samples")] HttpRequestData req,
		FunctionContext context)
	{
		var logger = context.GetLogger("SamplePost");
		var tableFactory = context.InstanceServices.GetService(typeof(Func<string, Azure.Data.Tables.TableClient>)) as Func<string, Azure.Data.Tables.TableClient>;
		if (tableFactory == null)
		{
			throw new InvalidOperationException("TableClient factory is not registered. Ensure AzureWebJobsStorage is configured or register a TableClient factory in Program.cs.");
		}
		var table = tableFactory(Constants.SamplesTableName);

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

		if (saveRequest == null)
		{
			response.StatusCode = System.Net.HttpStatusCode.BadRequest;
			await response.WriteStringAsync("Request payload required.");
			return response;
		}

		var id = Guid.NewGuid().ToString("N").Substring(24);

		if (saveRequest.Xaml == null || saveRequest.Xaml.Length < 5)
		{
			response.StatusCode = System.Net.HttpStatusCode.BadRequest;
			await response.WriteStringAsync("Xaml required.");
			return response;
		}

		if (saveRequest.Xaml.Length > 512 * 1024)
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

		var clientIp = req.GetClientIp();

		var sample = new Sample(id)
		{
			Data = saveRequest?.Data,
			Xaml = saveRequest?.Xaml,
			Category = Constants.DefaultCategoryIdForSaving,
			Title = saveRequest?.Title,
			IpAddress = clientIp,
			UserAgent = (req.Headers.TryGetValues("User-Agent", out var ua) ? string.Join(" ", ua) : null),
			App = saveRequest?.App
		};

		await table.AddEntityAsync(sample);

		response.StatusCode = System.Net.HttpStatusCode.OK;
		await response.WriteStringAsync(id);
		return response;
	}
}