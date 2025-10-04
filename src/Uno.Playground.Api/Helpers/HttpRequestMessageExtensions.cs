using System.Net.Http;

namespace Uno.Playground.Api.Helpers;

public static class HttpRequestMessageExtensions
{
	public static string? GetClientIp(this HttpRequestMessage request)
	{
		// Prefer X-Forwarded-For header (when behind proxies)
		if (request.Headers.TryGetValues("X-Forwarded-For", out var values))
		{
			var header = System.Linq.Enumerable.FirstOrDefault(values);
			if (!string.IsNullOrWhiteSpace(header))
			{
				// May contain a list of IPs, take the first
				return header.Split(',')[0].Trim();
			}
		}

		if (request.Options != null && request.Options.TryGetValue(new HttpRequestOptionsKey<object>("MS_HttpContext"), out var ctxObj))
		{
			try
			{
				dynamic ctx = ctxObj;
				var reqInner = ctx?.Request;
				if (reqInner != null)
				{
					// Try common property names
					var addr = reqInner.UserHostAddress as string;
					if (!string.IsNullOrWhiteSpace(addr))
					{
						return addr;
					}
				}
			}
			catch
			{
				// ignore any reflection/dynamic errors and return null
			}
		}

		return null;
	}
}
