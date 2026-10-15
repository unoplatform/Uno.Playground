using System.Runtime.InteropServices;
using Uno.UI.Hosting;

namespace Uno.UI.Demo
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
#if ENABLE_EXCEPTIONS_LOGGING
			MonoInternals.mono_trace_enable(1);
			MonoInternals.mono_trace_set_options("E:all");
#endif

			var host = UnoPlatformHostBuilder.Create()
				.App(() => new App())
				.UseWebAssembly()
				.Build();

			await host.RunAsync();
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
