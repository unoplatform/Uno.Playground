using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using Uno.UI.Demo.Api.Helpers;
using Uno.UI.Demo.Api.Models;

namespace Uno.UI.Demo.Api
{
	public static class SamplesGet
	{
		[FunctionName("SamplesGet")]
		public static async Task<HttpResponseMessage> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "samples")]
			HttpRequestMessage req,
			[Table(Constants.SamplesTableName)] CloudTable table,
			TraceWriter log,
			CancellationToken ct)
		{

			var categoriesQuery = table
				.CreateQuery<SampleCategory>()
				.Where(cat => cat.PartitionKey.Equals(nameof(SampleCategory)) && !cat.RowKey.Equals(Constants.DefaultCategoryIdForSaving))
				.AsTableQuery()
				.SelectColumn(nameof(SampleCategory.Title));

			var categories = (await table.ExecuteQuery(categoriesQuery, ct))
				.OrderBy(cat => cat.ListingOrder)
				.ThenBy(cat => cat.Title)
				.ThenBy(cat => cat.Id)
				.ToArray();

			var result = new List<SampleCategoryViewModel>(categories.Length);

			var etagSb = new StringBuilder();

			etagSb.Append('"');

			foreach (var category in categories)
			{
				var samplesQuery = table
					.CreateQuery<Sample>()
					.Where(smpl => smpl.PartitionKey.Equals(nameof(Sample)) && smpl.Category.Equals(category.Id))
					.AsTableQuery()
					.SelectColumn(nameof(Sample.Category))
					.SelectColumn(nameof(Sample.Title))
					.SelectColumn(nameof(Sample.Description))
					.SelectColumn(nameof(Sample.Keywords));

				var samples = (await table.ExecuteQuery(samplesQuery, ct))
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

			if (req.Headers.IfMatch.Any(im => im.Tag.Equals(etag, StringComparison.OrdinalIgnoreCase)))
			{
				return req.CreateResponse(HttpStatusCode.NotModified);
			}

			var response = req.CreateResponse(HttpStatusCode.OK, result.ToArray());
			response.Headers.CacheControl =
				new CacheControlHeaderValue
				{
					NoCache = false,
					Private = false,
					MaxAge = TimeSpan.FromHours(0.5),
					MustRevalidate = false,
					NoStore = false,
					NoTransform = true,
					MaxStale = true
				};
			response.Headers.ETag = new EntityTagHeaderValue(etag, false);

			return response;
		}
	}
}
