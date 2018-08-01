using UIKit;

namespace Uno.UI.Demo.iOS
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main(string[] args)
		{
#if !DEBUG
			// This improves the loading performance for local images in Uno.UI.
			UIApplication.CheckForIllegalCrossThreadCalls = false;
#endif

			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main(args, null, typeof(App));
		}
	}
}