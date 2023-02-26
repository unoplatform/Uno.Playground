using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Uno.UI.Demo.AspnetShell.Helpers
{
	public static class HttpRequestDataExtensions
	{
		public static HttpResponseData CreateResponse(this HttpRequestData request, HttpStatusCode statusCode, string content)
		{
			var response = request.CreateResponse(statusCode);
			response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
			response.WriteString(content);

			return response;
		}

		public static string GetClientIp(this HttpRequestData request)
		{
			// https://github.com/Azure/Azure-Functions/issues/1597#issuecomment-753373366
			if (request.Headers.TryGetValues("X-Forwarded-For", out IEnumerable<string> values))
			{
				var ipn = values.FirstOrDefault().Split(new char[] { ',' }).FirstOrDefault().Split(new char[] { ':' }).FirstOrDefault();
				return ipn;
			}

			return "N/A (X-Forwarded-For not available)";
		}

		public static string GetUserAgent(this HttpRequestData request)
		{
			if (request.Headers.TryGetValues("User-Agent", out IEnumerable<string> values))
			{
				return string.Join(" ", values);
			}

			return null;
		}
	}
}
