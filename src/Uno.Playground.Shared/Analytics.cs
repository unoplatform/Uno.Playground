using System;
using Windows.UI.Xaml.Navigation;

namespace Uno.UI.Demo
{
	public static partial class Analytics
	{
		public static void ReportPageView(NavigationEventArgs pageType)
		{
			InnerReportPageView(pageType.SourcePageType.FullName, pageType.Parameter?.ToString());
		}

		public static void ReportPageView(string paneName, string pageParam)
		{
			InnerReportPageView(paneName, pageParam);
		}

		static partial void InnerReportPageView(string pageName, string pageParam);
	}
}