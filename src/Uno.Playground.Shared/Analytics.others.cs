#if !__WASM__

using System.Collections.Generic;

namespace Uno.UI.Demo
{
	static partial class Analytics
	{
		static partial void InnerReportPageView(string pageName, string arguments)
		{
			Microsoft.AppCenter.Analytics.Analytics.TrackEvent(
				"Navigate",
				new Dictionary<string, string> { 
					{ "PageName", pageName },
					{ "Arguments", arguments }
				}
			);
		}
	}
}
#endif