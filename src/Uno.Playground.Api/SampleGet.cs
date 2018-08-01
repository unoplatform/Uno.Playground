using System.Linq;
using System.Net;
using System.Net.Http;
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
	public static class SampleGet
	{
		[FunctionName("SampleGet")]
		public static async Task<HttpResponseMessage> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "samples/{id}")]
			HttpRequestMessage req,
			[Table(Constants.SamplesTableName)] CloudTable table,
			TraceWriter log,
			string id,
			CancellationToken ct)
		{
			var sampleQuery = table
				.CreateQuery<Sample>()
				.Where(smpl => smpl.PartitionKey.Equals(nameof(Sample)) && smpl.RowKey.Equals(id))
				.AsTableQuery();

			var queryResult = await table.ExecuteQuery(sampleQuery, ct);

			var sample = queryResult.Select(s => new SampleDetailViewModel(s)).FirstOrDefault();

			return sample == null
				? req.CreateErrorResponse(HttpStatusCode.NotFound, "Sample not found")
				: req.CreateResponse(HttpStatusCode.OK, sample);
		}
	}
}
