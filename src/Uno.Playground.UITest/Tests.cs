using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using UITests.Helpers;
using UITests.Queries;
using Uno.Playground.UITest.Extensions;

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

		[Test]
		public void Playground()
		{

		}

		[Test]
		public void MessageDialog()
		{
			app.WaitForElement("CheckBox");
			app.ScrollDownTo("MessageDialog", "ScrollViewer", ScrollStrategy.Gesture, 0.8, 1000, true);

			app.Tap("MessageDialog");


		}

		[Test]
		public void PasswordBox()
		{
			app.WaitForElement("CheckBox");
			app.ScrollDownTo("PasswordBox", "ScrollViewer", ScrollStrategy.Gesture, 0.8, 1000, true);

			app.Tap("PasswordBox");
			app.WaitForElement("PasswordBoxEnabled");
			app.ClearText();

			app.EnterTextSlowly("TestPassword", 0.2f);
			app.TouchAndHold("RevealButton");

			app.Query(c => c.Marked("PasswordBoxDisabled").Property("Enabled", false));

			app.Tap("PasswordBoxAnimated");
			app.EnterTextSlowly("TestPassword", 0.2f);
			app.TouchAndHold("RevealButton");

			AppResult[] result = app.Query("TestPassword");
			Assert.IsTrue(result.Any());

		}
	}
}
