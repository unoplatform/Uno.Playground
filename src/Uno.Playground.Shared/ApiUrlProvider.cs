using System;
using Windows.Storage;

namespace Uno.Playground;

/// <summary>
/// Resolves the API base URL for the Playground at runtime.
/// Priority order:
/// 1) Query string parameter "apiUrl" (persisted to LocalSettings during the session).
/// 2) Previously persisted value in LocalSettings.
/// 3) If running under WASM on localhost, use the local Aspire API on the fixed port.
/// 4) Fallback to the public Uno API default.
/// </summary>
public static class ApiUrlProvider
{
	private const string SettingsKey = "Playground.ApiBaseUrl";
	private const string QueryParamKey = "apiUrl";
	private static string? _cached;

	public static string GetBaseApiUrl()
	{
		if (!string.IsNullOrWhiteSpace(_cached))
		{
			return _cached!;
		}

		// 1) Query string override
		var fromQuery = TryGetQueryParam(QueryParamKey);
		if (!string.IsNullOrWhiteSpace(fromQuery))
		{
			fromQuery = Normalize(fromQuery!);
			SaveSetting(fromQuery!);
			return _cached = fromQuery!;
		}

		// 2) Persisted setting
		try
		{
			var settings = ApplicationData.Current?.LocalSettings;
			if (settings != null && settings.Values.TryGetValue(SettingsKey, out var v) && v is string s && !string.IsNullOrWhiteSpace(s))
			{
				return _cached = s;
			}
		}
		catch
		{
			// Ignore settings access errors
		}

		// 3) Localhost heuristic (when served locally via Aspire)
		var localhostUrl = GetLocalApiUrlIfApplicable();
		if (!string.IsNullOrWhiteSpace(localhostUrl))
		{
			SaveSetting(localhostUrl!);
			return _cached = localhostUrl!;
		}

		// 4) Default public API
		return _cached = PlaygroundConstants.BaseApiUrl;
	}

	private static void SaveSetting(string url)
	{
		try
		{
			ApplicationData.Current.LocalSettings.Values[SettingsKey] = url;
		}
		catch
		{
			// Ignore settings write errors
		}
	}

	private static string Normalize(string url)
	{
		return url.Trim();
	}

	private static string? TryGetQueryParam(string key)
	{
#if __WASM__
		try
		{
			var search = Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.search") ?? string.Empty;
			if (string.IsNullOrEmpty(search))
			{
				return null;
			}
			if (search.StartsWith("?"))
			{
				search = search.Substring(1);
			}
			var parts = search.Split('&');
			foreach (var part in parts)
			{
				if (string.IsNullOrWhiteSpace(part))
				{
					continue;
				}
				var kv = part.Split('=', 2);
				var k = Uri.UnescapeDataString(kv[0]);
				if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
				{
					var val = kv.Length == 2 ? Uri.UnescapeDataString(kv[1]) : string.Empty;
					return val;
				}
			}
		}
		catch
		{
			// Ignore query parsing errors
		}
#endif
		return null;
	}

	private static string? GetLocalApiUrlIfApplicable()
	{
#if __WASM__
		try
		{
			var host = Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.hostname")?.Trim();
			if (!string.IsNullOrEmpty(host) && (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) || string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)))
			{
				// Fixed port configured in AppHost.cs for the API endpoint.
				return "http://localhost:54050/api/samples";
			}
		}
		catch
		{
			// Ignore detection errors
		}
#endif
		return null;
	}
}
