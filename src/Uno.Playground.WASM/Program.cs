using System;
using System.Runtime.InteropServices;

namespace Uno.UI.Demo
{
	public class Program
	{
		private static App _app;

		public static void Main(string[] args)
		{
			MonoInternals.mono_trace_enable(1);
			MonoInternals.mono_trace_set_options("E:all");

			Windows.UI.Xaml.Application.Start(_ => _app = new App());
		}
	}

	static class MonoInternals
	{
		[DllImport("__Native")]
		internal static extern void mono_trace_enable(int enable);
		[DllImport("__Native")]
		internal static extern int mono_trace_set_options(string options);
	}
}
