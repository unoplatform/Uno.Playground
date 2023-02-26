using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Uno.UI.Demo.Api.Models;
using Uno.UI.Demo.AspnetShell.Helpers;
using System.Text.Json;
using Azure.Data.Tables;
using System;

namespace Uno.UI.Demo.Api
{
	public class SampleSave
	{
		private readonly ILogger _logger;

		public SampleSave(ILoggerFactory loggerFactory)
			=> _logger = loggerFactory.CreateLogger<SampleSave>();

		[Function("SampleSave")]
		public async Task<HttpResponseData> Run(
			[HttpTrigger(AuthorizationLevel.Admin, "put", Route = "samples/{id}")] HttpRequestData req,
			string id
		)
		{
			var clientIp = req.GetClientIp();

			var saveRequest =
				JsonSerializer.Deserialize<SampleSaveRequest>(await req.ReadAsStringAsync());

			var tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), Constants.SamplesTableName);

			var sampleQuery = tableClient.QueryAsync<Sample>(smpl => smpl.PartitionKey.Equals(nameof(Sample)) && smpl.RowKey.Equals(id));

			var existing = await sampleQuery
				.FirstOrDefaultAsync();

			var sample = existing ?? new Sample(id);

			var exists = existing != null;

			if (saveRequest == null)
			{
				return req.CreateResponse(HttpStatusCode.BadRequest, "Request payload required.");
			}

			if (!exists && (saveRequest.Xaml == null || saveRequest.Xaml.Length < 5))
			{
				return req.CreateResponse(HttpStatusCode.BadRequest, "Xaml required.");
			}

			if (saveRequest.Xaml != null && saveRequest.Xaml.Length > 512 * 1024)
			{
				return req.CreateResponse(HttpStatusCode.RequestEntityTooLarge, "Xaml too big.");
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
				sample.IpAddress = clientIp;
			}
			if (string.IsNullOrWhiteSpace(sample.UserAgent))
			{
				sample.UserAgent = req.GetUserAgent();
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

			if (existing == null)
			{
				await tableClient.AddEntityAsync(sample);
			}
			else
			{
				await tableClient.UpdateEntityAsync(sample, sample.ETag);
			}

			return req.CreateResponse(HttpStatusCode.OK, id);
		}
	}
}
