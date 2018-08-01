using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Uno.UI.Demo.Api.Models;
using Uno.UI.Demo.AspnetShell.Helpers;

namespace Uno.UI.Demo.Api
{
	public static class SamplePost
	{
		[FunctionName("SamplePost")]
		public static async Task<HttpResponseMessage> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "samples")]
			HttpRequestMessage req,
			[Table(Constants.SamplesTableName)] CloudTable table,
			TraceWriter log,
			CancellationToken ct)
		{
			var saveRequest =
				JsonConvert.DeserializeObject<SampleSaveRequest>(await req.Content.ReadAsStringAsync());

			if (saveRequest == null)
			{
				return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Request payload required.");
			}

			var id = Guid.NewGuid().ToString("N").Substring(24); // 8 characters is enough

			if (saveRequest.Xaml == null || saveRequest.Xaml.Length < 5)
			{
				return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Xaml required.");
			}

			if (saveRequest.Xaml.Length > 512 * 1024)
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

			var sample = new Sample(id)
			{
				Data = saveRequest.Data,
				Xaml = saveRequest.Xaml,
				Category = Constants.DefaultCategoryIdForSaving,
				Title = saveRequest.Title,
				IpAddress = req.GetClientIp(),
				UserAgent = req.Headers.UserAgent.ToString(),
				App = saveRequest.App
			};

			var operation = new TableBatchOperation();
			operation.Insert(sample);
			await table.ExecuteBatchAsync(operation, null, null, ct);

			return req.CreateResponse(HttpStatusCode.OK, id);
		}
	}
}
