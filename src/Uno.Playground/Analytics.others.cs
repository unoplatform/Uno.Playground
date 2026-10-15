#if !__WASM__

namespace Uno.UI.Demo
{
	static partial class Analytics
	{
		static partial void InnerReportPageView(string pageName, string? pageParam)
		{
			// No analytics backend outside of WebAssembly (AppCenter was retired).
			System.Diagnostics.Debug.WriteLine($"Navigate: {pageName} ({pageParam})");
		}
	}
}
#endif