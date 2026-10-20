using Uno.UI.Hosting;

namespace Uno.UI.Demo
{
	public class Program
	{
		[STAThread]
		public static void Main(string[] args)
		{
			var host = UnoPlatformHostBuilder.Create()
				.App(() => new App())
				.UseX11()
				//.UseLinuxFrameBuffer() // no WebView on this backend, so the Monaco editor can never start
				.UseMacOS()
				.UseWin32()
				.Build();

			host.Run();
		}
	}
}
