using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Tables;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;
using Uno.UI.Demo.Api.Models;

namespace Uno.UI.Demo.Api
{
	public class SamplePost
	{
		private readonly ILogger<SamplePost> _logger;
		private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

		public SamplePost(ILogger<SamplePost> logger)
		{
			_logger = logger;
		}

		[Function("SamplePost")]
		public async Task<HttpResponseData> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "samples")]
			HttpRequestData req,
			[TableInput(Constants.SamplesTableName)] TableClient table,
			CancellationToken ct)
		{
			ArgumentNullException.ThrowIfNull(req);
			ArgumentNullException.ThrowIfNull(table);
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			var saveRequest = JsonSerializer.Deserialize<SampleSaveRequest>(requestBody, SerializerOptions);

			if (saveRequest == null)
			{
				var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
				await badRequest.WriteStringAsync("Request payload required.");
				return badRequest;
			}

			var id = Guid.NewGuid().ToString("N").Substring(24); // 8 characters is enough

			if (saveRequest.Xaml == null || saveRequest.Xaml.Length < 5)
			{
				var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
				await badRequest.WriteStringAsync("Xaml required.");
				return badRequest;
			}

			if (saveRequest.Xaml.Length > 512 * 1024)
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

			// Note: In Isolated Worker, getting Client IP is different.
			// It might be in headers like X-Forwarded-For.
			// For now, we'll try to get it from headers if available, or null.
			string? clientIp = null;
			if (req.Headers.TryGetValues("X-Forwarded-For", out var forwardedFor))
			{
				clientIp = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
			}

			string? userAgent = null;
			if (req.Headers.TryGetValues("User-Agent", out var ua))
			{
				userAgent = ua.FirstOrDefault();
			}

			var sample = new Sample(id)
			{
				Data = saveRequest.Data ?? string.Empty,
				Xaml = saveRequest.Xaml,
				Category = Constants.DefaultCategoryIdForSaving,
				Title = saveRequest.Title ?? string.Empty,
				IpAddress = clientIp ?? string.Empty,
				UserAgent = userAgent ?? string.Empty,
				App = saveRequest.App ?? string.Empty
			};

			await table.AddEntityAsync(sample, cancellationToken: ct);

			var response = req.CreateResponse(HttpStatusCode.OK);
			await response.WriteAsJsonAsync(id);
			return response;
		}
	}
}
