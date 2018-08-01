using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;

namespace Uno.UI.Demo.AspnetShell.Helpers
{
	public static class HttpRequestMessageExtensions
	{
		public static string GetClientIp(this HttpRequestMessage request)
		{
			if (request.Properties.ContainsKey("MS_HttpContext"))
			{
				return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
			}

			if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
			{
				RemoteEndpointMessageProperty prop;
				prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
				return prop.Address;
			}

			return null;
		}
	}
}
