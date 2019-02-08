using System;

namespace Uno.UI.Demo
{
	public class Program
	{
		private static App _app;

		public static int Main(string[] args)
		{
			// Uno.UI.FeatureConfiguration.Interop.ForceJavascriptInterop = true;

			Windows.UI.Xaml.Application.Start(_ => _app = new App());

			return 0;
		}
	}
}
