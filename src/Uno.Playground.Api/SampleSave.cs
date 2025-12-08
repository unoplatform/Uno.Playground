using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Tables;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Data.Tables;
using Uno.UI.Demo.Api.Models;

namespace Uno.UI.Demo.Api
{
	public class SampleSave
	{
		private readonly ILogger<SampleSave> _logger;
		private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

		public SampleSave(ILogger<SampleSave> logger)
		{
			_logger = logger;
		}

		[Function("SampleSave")]
		public async Task<HttpResponseData> Run(
			[HttpTrigger(AuthorizationLevel.Admin, "put", Route = "samples/{id}")]
			HttpRequestData req,
			[TableInput(Constants.SamplesTableName)] TableClient table,
			string id,
			CancellationToken ct)
		{
			ArgumentNullException.ThrowIfNull(req);
			ArgumentNullException.ThrowIfNull(table);
			ArgumentException.ThrowIfNullOrEmpty(id);

			// Note: In Isolated Worker, getting Client IP is different.
			// It might be in headers like X-Forwarded-For.
			string? clientIp = null;
			if (req.Headers.TryGetValues("X-Forwarded-For", out var forwardedFor))
			{
				clientIp = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
			}

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			SampleSaveRequest? saveRequestCandidate = JsonSerializer.Deserialize<SampleSaveRequest>(requestBody, SerializerOptions);

			if (saveRequestCandidate is null)
			{
				var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
				await badRequest.WriteStringAsync("Request payload required.");
				return badRequest;
			}

			SampleSaveRequest saveRequest = saveRequestCandidate!;

			var existing = await table.GetEntityIfExistsAsync<Sample>(nameof(Sample), id, cancellationToken: ct);
			Sample sample = existing.HasValue && existing.Value is Sample s ? s : new Sample(id);
			var exists = existing.HasValue;

			if (!exists && (saveRequest.Xaml == null || saveRequest.Xaml.Length < 5))
			{
				var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
				await badRequest.WriteStringAsync("Xaml required.");
				return badRequest;
			}

			if (saveRequest.Xaml != null && saveRequest.Xaml.Length > 512 * 1024)
			{
				var tooLarge = req.CreateResponse(HttpStatusCode.RequestEntityTooLarge);
				await tooLarge.WriteStringAsync("Xaml too big.");
				return tooLarge;
			}

			if (saveRequest.Data != null && saveRequest.Data.Length > 16 * 1024)
			{
				var tooLarge = req.CreateResponse(HttpStatusCode.RequestEntityTooLarge);
				await tooLarge.WriteStringAsync("Data too big.");
				return tooLarge;
			}

			if (saveRequest.Title != null && saveRequest.Title.Length > 255)
			{
				var tooLarge = req.CreateResponse(HttpStatusCode.RequestEntityTooLarge);
				await tooLarge.WriteStringAsync("Title too big.");
				return tooLarge;
			}

			if (saveRequest.App != null && saveRequest.App.Length > 255)
			{
				var tooLarge = req.CreateResponse(HttpStatusCode.RequestEntityTooLarge);
				await tooLarge.WriteStringAsync("App name too big.");
				return tooLarge;
			}


			if (!string.IsNullOrWhiteSpace(saveRequest.Data))
			{
				sample.Data = saveRequest.Data!;
			}

			if (!string.IsNullOrWhiteSpace(saveRequest.Xaml))
			{
				sample.Xaml = saveRequest.Xaml!;
			}

			var category = saveRequest.Category;
			if (string.IsNullOrWhiteSpace(sample.Category) || !string.IsNullOrWhiteSpace(category))
			{
				sample.Category = string.IsNullOrWhiteSpace(category)
					? Constants.DefaultCategoryIdForSaving
					: category;
			}
			if (!string.IsNullOrWhiteSpace(saveRequest.Title))
			{
				sample.Title = saveRequest.Title!;
			}
			if (string.IsNullOrWhiteSpace(sample.IpAddress))
			{
				sample.IpAddress = clientIp ?? string.Empty;
			}
			if (string.IsNullOrWhiteSpace(sample.UserAgent))
			{
				string? userAgent = null;
				if (req.Headers.TryGetValues("User-Agent", out var ua))
				{
					userAgent = ua.FirstOrDefault();
				}
				sample.UserAgent = userAgent ?? string.Empty;
			}
			if (!string.IsNullOrWhiteSpace(saveRequest.App))
			{
				sample.App = saveRequest.App ?? string.Empty;
			}
			if (!string.IsNullOrWhiteSpace(saveRequest.PathData))
			{
				sample.PathData = saveRequest.PathData ?? string.Empty;
			}
			if (!string.IsNullOrWhiteSpace(saveRequest.AccentPathData))
			{
				sample.AccentPathData = saveRequest.AccentPathData ?? string.Empty;
			}

			if (!exists)
			{
				await table.AddEntityAsync(sample, cancellationToken: ct);
			}
			else
			{
				await table.UpdateEntityAsync(sample, ETag.All, TableUpdateMode.Merge, cancellationToken: ct);
			}

			var response = req.CreateResponse(HttpStatusCode.OK);
			await response.WriteAsJsonAsync(id);
			return response;
		}
	}
}
