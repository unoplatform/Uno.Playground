using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Uno.UI.Demo.Api.Models;
using Uno.UI.Demo.AspnetShell.Helpers;
using Azure.Data.Tables;
using System.Text.Json;

namespace Uno.UI.Demo.Api;

public class SamplePost
{
	private readonly ILogger _logger;

	public SamplePost(ILoggerFactory loggerFactory)
		=> _logger = loggerFactory.CreateLogger<SamplePost>();

	[Function("SamplePost")]
	public async Task<HttpResponseData> Run(
		[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "samples")] HttpRequestData req
	)
	{
		var tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), Constants.SamplesTableName);

		string json = await req.ReadAsStringAsync();

		var saveRequest =
			JsonSerializer.Deserialize<SampleSaveRequest>(json);

		var id = Guid.NewGuid().ToString("N").Substring(24); // 8 characters is enough

		if (saveRequest.Xaml == null || saveRequest.Xaml.Length < 5)
		{
			return req.CreateResponse(HttpStatusCode.BadRequest, "Xaml required.");
		}

		if (saveRequest.Xaml.Length > 512 * 1024)
		{
			return req.CreateResponse(HttpStatusCode.RequestEntityTooLarge, "Xaml too big.");
		}

		if (saveRequest.Code.Length > 512 * 1024)
		{
			return req.CreateResponse(HttpStatusCode.RequestEntityTooLarge, "Code is too big.");
		}

		if (saveRequest.Data != null && saveRequest.Data.Length > 16 * 1024)
		{
			return req.CreateResponse(HttpStatusCode.RequestEntityTooLarge, "Data too big.");
		}

		if (saveRequest.Title != null && saveRequest.Title.Length > 255)
		{
			return req.CreateResponse(HttpStatusCode.RequestEntityTooLarge, "Title too big.");
		}

		if (saveRequest.App != null && saveRequest.App.Length > 255)
		{
			return req.CreateResponse(HttpStatusCode.RequestEntityTooLarge, "App name too big.");
		}

		var sample = new Sample(id)
		{
			Data = saveRequest.Data,
			Code = saveRequest.Code,
			Xaml = saveRequest.Xaml,
			Category = Constants.DefaultCategoryIdForSaving,
			Title = saveRequest.Title,
			IpAddress = req.GetClientIp(),
			UserAgent = req.GetUserAgent(),
			App = saveRequest.App,
			Timestamp = DateTimeOffset.UtcNow,
		};

		await tableClient.AddEntityAsync(sample);

		return req.CreateResponse(HttpStatusCode.OK, JsonSerializer.Serialize(id));
	}
}
