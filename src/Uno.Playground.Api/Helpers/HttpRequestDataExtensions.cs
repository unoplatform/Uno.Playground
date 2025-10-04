using System.Linq;
using Microsoft.Azure.Functions.Worker.Http;

namespace Uno.Playground.Api.Helpers;

public static class HttpRequestDataExtensions
{
	public static string? GetClientIp(this HttpRequestData request)
	{
		// Prefer X-Forwarded-For header (when behind proxies)
		if (request.Headers.TryGetValues("X-Forwarded-For", out var values))
		{
			var header = values.FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(header))
			{
				return header.Split(',')[0].Trim();
			}
		}

		// Fallback to X-Forwarded-For alternative casing
		if (request.Headers.TryGetValues("x-forwarded-for", out var values2))
		{
			var header = values2.FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(header))
			{
				return header.Split(',')[0].Trim();
			}
		}

		return null;
	}
}