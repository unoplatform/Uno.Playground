using System;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Uno.Playground.UITest
{
    public class AppInitializer
    {
        public static IApp StartApp(Platform platform)
        {
            if (platform == Platform.Android)
            {
				return ConfigureApp
					.Android
					.InstalledApp("com.nventive.uno.ui.demo")
					.EnableLocalScreenshots()
					.Debug()
					.StartApp();

            }

            return ConfigureApp
				.iOS
				.StartApp();
        }
    }
}
