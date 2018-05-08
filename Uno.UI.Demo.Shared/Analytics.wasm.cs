#if __WASM__
using Uno.Foundation;

namespace Uno.UI.Demo
{
	static partial class Analytics
	{
		static partial void InnerReportPageView(string pageName, string arguments)
		{
			var command = string.Concat(
				"Uno.UI.Demo.Analytics.reportPageView(\"",
				WebAssemblyRuntime.EscapeJs(pageName),
				"\");");

			WebAssemblyRuntime.InvokeJS(command);
		}
	}
}
#endif