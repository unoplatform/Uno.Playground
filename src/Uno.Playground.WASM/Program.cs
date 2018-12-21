using System;

namespace Uno.UI.Demo
{
	public class Program
	{
		private static App _app;

		public static void Main(string[] args)
		{
			Windows.UI.Xaml.Application.Start(_ => _app = new App());
		}
	}
}
