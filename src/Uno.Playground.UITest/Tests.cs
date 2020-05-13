using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using UITests.Helpers;
using UITests.Queries;

namespace Uno.Playground.UITest
{
    [TestFixture(Platform.Android)]
    //[TestFixture(Platform.iOS)]
    public class Tests
    {
        IApp app;
        Platform platform;

        public Tests(Platform platform)
        {
            this.platform = platform;
        }

        [SetUp]
        public void BeforeEachTest()
        {
            app = AppInitializer.StartApp(platform);
        }

		[Test]
		public void Smoke()
		{
			app.Repl();
			
		}

		[Test]
		public void CheckBox()
		{
			app.WaitForElement("CheckBox");
			app.Tap("CheckBox");

			app.WaitForElement("CheckBox");
			app.Tap("CheckBox2");

			var target = app.Marked("CheckBox2");
			//var result = target?.GetDependencyPropertyValue("IsChecked");
			/* put a break point on the line after, use debug instead of run to observe this value */

			//app.Wait(30);





		}
	}
}
