using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UITests.Helpers;
using Xamarin.UITest;

namespace Uno.Playground.UITest.Extensions
{
	public static class AppExtensions
	{
		public static void EnterTextSlowly(this IApp app, string text, float delayBetweenInput = 0.5f)
		{
			foreach (var c in text)
			{
				app.EnterText(c.ToString());
				app.Wait(delayBetweenInput);
			}
		}
	}
}
