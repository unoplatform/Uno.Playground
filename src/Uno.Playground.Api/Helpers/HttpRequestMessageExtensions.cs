using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Net.Http;
using System.Web;

namespace Uno.UI.Demo.AspnetShell.Helpers
{
	public static class HttpRequestMessageExtensions
	{
		public static string GetClientIp(this HttpRequestData request)
		{
			//if (request.Properties.ContainsKey("MS_HttpContext"))
			//{
			//	return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
			//}

			//if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
			//{
			//	RemoteEndpointMessageProperty prop;
			//	prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
			//	return prop.Address;
			//}

			return null;
		}
		public static string GetUserAgent(this HttpRequestData request)
		{
			return "";
		}
	}

	public static class HttpRequestDataExtensions
	{
		public static HttpResponseData CreateResponse(this HttpRequestData request, HttpStatusCode statusCode, string content)
		{
			var response = request.CreateResponse(statusCode);
			response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
			response.WriteString(content);

			return response;
		}
	}
}
