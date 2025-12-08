using System.Linq;
using System.Net;
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
	public class SampleGet
	{
		private readonly ILogger<SampleGet> _logger;

		public SampleGet(ILogger<SampleGet> logger)
		{
			_logger = logger;
		}

		[Function("SampleGet")]
		public async Task<HttpResponseData> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "samples/{id}")]
			HttpRequestData req,
			[TableInput(Constants.SamplesTableName)] TableClient table,
			string id,
			CancellationToken ct)
		{
			ArgumentNullException.ThrowIfNull(req);
			ArgumentNullException.ThrowIfNull(table);
			ArgumentException.ThrowIfNullOrEmpty(id);

			var sampleQuery = table.QueryAsync<Sample>(s => s.PartitionKey == nameof(Sample) && s.RowKey == id, cancellationToken: ct);

			Sample? sample = null;
			await foreach (var s in sampleQuery)
			{
				sample = s;
				break;
			}

			if (sample == null)
			{
				var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
				await notFoundResponse.WriteStringAsync("Sample not found");
				return notFoundResponse;
			}

			var response = req.CreateResponse(HttpStatusCode.OK);
			await response.WriteAsJsonAsync(new SampleDetailViewModel(sample));
			return response;
		}
	}
}
