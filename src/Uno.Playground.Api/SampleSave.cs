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
using Newtonsoft.Json;
using Uno.UI.Demo.Api.Helpers;
using Uno.UI.Demo.Api.Models;
using Uno.UI.Demo.AspnetShell.Helpers;

namespace Uno.UI.Demo.Api
{
	public static class SampleSave
	{
		[FunctionName("SampleSave")]
		public static async Task<HttpResponseMessage> Run(
			[HttpTrigger(AuthorizationLevel.Admin, "put", Route = "samples/{id}")]
			HttpRequestMessage req,
			[Table(Constants.SamplesTableName)] CloudTable table,
			string id,
			TraceWriter log,
			CancellationToken ct)
		{
			var clientIp = req.GetClientIp();

			var saveRequest =
				JsonConvert.DeserializeObject<SampleSaveRequest>(await req.Content.ReadAsStringAsync());


			var existingQuery = table
				.CreateQuery<Sample>()
				.Where(smpl => smpl.PartitionKey.Equals(nameof(Sample)) && smpl.RowKey.Equals(id))
				.AsTableQuery();

			var existing = (await table.ExecuteQuery(existingQuery, ct)).FirstOrDefault();
			var sample = existing ?? new Sample(id);

			var exists = existing != null;

			if (saveRequest == null)
			{
				return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Request payload required.");
			}

			if (!exists && (saveRequest.Xaml == null || saveRequest.Xaml.Length < 5))
			{
				return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Xaml required.");
			}

			if (saveRequest.Xaml != null && saveRequest.Xaml.Length > 512 * 1024)
			{
				return req.CreateErrorResponse(HttpStatusCode.RequestEntityTooLarge, "Xaml too big.");
			}

			if (saveRequest.Data != null && saveRequest.Data.Length > 16 * 1024)
			{
				return req.CreateErrorResponse(HttpStatusCode.RequestEntityTooLarge, "Data too big.");
			}

			if (saveRequest.Title != null && saveRequest.Title.Length > 255)
			{
				return req.CreateErrorResponse(HttpStatusCode.RequestEntityTooLarge, "Title too big.");
			}

			if (saveRequest.App != null && saveRequest.App.Length > 255)
			{
				return req.CreateErrorResponse(HttpStatusCode.RequestEntityTooLarge, "App name too big.");
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
				sample.UserAgent = req.Headers.UserAgent.ToString();
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

			var operation = new TableBatchOperation();
			if (existing == null)
			{
				operation.Insert(sample);
			}
			else
			{
				operation.Merge(sample);
			}
			await table.ExecuteBatchAsync(operation, null, null, ct);

			return req.CreateResponse(HttpStatusCode.OK, id);
		}
	}
}