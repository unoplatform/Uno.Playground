using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
	public class SamplesGet
	{
		private readonly ILogger<SamplesGet> _logger;

		public SamplesGet(ILogger<SamplesGet> logger)
		{
			_logger = logger;
		}

		[Function("SamplesGet")]
		public async Task<HttpResponseData> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "samples")]
			HttpRequestData req,
			[TableInput(Constants.SamplesTableName)] TableClient table,
			CancellationToken ct)
		{

			ArgumentNullException.ThrowIfNull(req);
			ArgumentNullException.ThrowIfNull(table);

			await table.CreateIfNotExistsAsync(ct);

			var categoriesQuery = table.QueryAsync<SampleCategory>(
				filter: cat => cat.PartitionKey == nameof(SampleCategory) && cat.RowKey != Constants.DefaultCategoryIdForSaving,
				select: new[] { nameof(SampleCategory.Title), nameof(SampleCategory.RowKey), nameof(SampleCategory.PartitionKey), nameof(SampleCategory.ListingOrder) },
				cancellationToken: ct
			);

			var categoriesList = new List<SampleCategory>();
			await foreach (var cat in categoriesQuery)
			{
				categoriesList.Add(cat);
			}

			var categories = categoriesList
				.OrderBy(cat => cat.ListingOrder)
				.ThenBy(cat => cat.Title)
				.ThenBy(cat => cat.Id)
				.ToArray();

			var result = new List<SampleCategoryViewModel>(categories.Length);

			var etagSb = new StringBuilder();

			etagSb.Append('"');

			foreach (var category in categories)
			{
				var samplesQuery = table.QueryAsync<Sample>(
					filter: smpl => smpl.PartitionKey == nameof(Sample) && smpl.Category == category.Id,
					select: new[] { nameof(Sample.Category), nameof(Sample.Title), nameof(Sample.Description), nameof(Sample.Keywords), nameof(Sample.RowKey), nameof(Sample.PartitionKey), nameof(Sample.ListingOrder) },
					cancellationToken: ct
				);

				var samplesList = new List<Sample>();
				await foreach (var s in samplesQuery)
				{
					samplesList.Add(s);
				}

				var samples = samplesList
					.OrderBy(cat => cat.ListingOrder)
					.ThenBy(cat => cat.Title)
					.ThenBy(cat => cat.Id)
					.ToArray();

				var categoryVm = new SampleCategoryViewModel(category, samples);
				result.Add(categoryVm);
				etagSb.Append(categoryVm.SamplesHash.ToString("X"));
			}

			etagSb.Append('"');

			var etag = etagSb.ToString();

			if (req.Headers.TryGetValues("If-Match", out var ifMatch) && ifMatch.Any(im => im.Equals(etag, StringComparison.OrdinalIgnoreCase)))
			{
				return req.CreateResponse(HttpStatusCode.NotModified);
			}

			var response = req.CreateResponse(HttpStatusCode.OK);
			await response.WriteAsJsonAsync(result.ToArray());
			
			response.Headers.Add("Cache-Control", "public, max-age=1800, max-stale"); // Simplified Cache-Control
			response.Headers.Add("ETag", etag);

			return response;
		}
	}
}
