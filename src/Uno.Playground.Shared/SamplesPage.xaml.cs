using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.Extensions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.UI.Demo
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class SamplesPage : Page
	{
		public SamplesPage()
		{
			this.InitializeComponent();

			Initialize();

			//Loaded += SamplesPage_Loaded;
			//UpdateCollapsibleCommandBar(0);
		}

		private void Initialize()
		{
			var featuredSamples = new Sample[]
			{
				new Sample
				{
					Title = "Playground",
					Description = "Try your own XAML live!",
					IconAccentPath = "M35.596 22.32l-2.81 2.87c-2.453 2.475-3.96 3.484-5.204 3.484h-.004c-1.23-.002-2.66-.96-5.1-3.422l-.587-.587 5.149-5.25c2.531-2.553 4.1-4.594 4.105-6.879 0-.33-.032-.654-.094-.976l.002.008c1.65.856 3.23 2.402 4.523 3.787l.026.028c1.196 1.206 1.802 2.2 1.906 3.125.127 1.138-.496 2.383-1.912 3.811zm-7.642 8.808c-.868 1.583-2.332 3.107-3.645 4.352l-.027.026c-2.228 2.248-3.983 2.913-6.877-.006l-2.847-2.834c-2.455-2.477-3.456-3.999-3.454-5.253.003-1.242.953-2.683 3.394-5.145l.756-.77 5.5 5.5c2.53 2.554 4.555 4.137 6.82 4.141h.009c.136 0 .27-.006.405-.017a.254.254 0 0 1-.034.006zm-23.56-6.88l-.027-.028c-1.196-1.206-1.802-2.2-1.906-3.125-.128-1.138.496-2.383 1.911-3.811l2.81-2.872c2.453-2.474 3.961-3.484 5.204-3.484h.005c1.23.003 2.66.961 5.1 3.423l.678.677h-.005l-5.398 5.501c-2.953 2.98-4.317 5.144-4.09 7.377-1.563-.876-3.053-2.34-4.283-3.659zm18.694-4.308l-2.932 2.99-3.183-3.184 2.93-2.986 3.185 3.18zM12.048 8.475c.88-1.463 2.227-2.851 3.448-4.01l.027-.026c2.228-2.248 3.983-2.914 6.878.005l2.847 2.835c2.455 2.476 3.456 3.998 3.453 5.253-.002 1.241-.952 2.682-3.394 5.145l-.5.51-5.592-5.583c-2.531-2.553-4.555-4.136-6.82-4.14h-.01c-.116 0-.232.005-.347.013l.01-.002zm25.295 5.179c-2.031-2.178-4.725-4.695-7.743-5.142-.684-.941-1.575-1.917-2.63-2.982l-2.847-2.834c-4.62-4.66-8.097-2.246-10.314-.014-1.968 1.868-4.214 4.29-4.933 7.008l.012-.007c-1.083.723-2.2 1.743-3.439 2.992l-2.81 2.871C.669 17.534-.184 19.44.033 21.372c.222 1.975 1.582 3.556 2.593 4.577 1.933 2.073 4.465 4.451 7.306 5.062.69 1.033 1.657 2.145 2.904 3.403l2.847 2.834c1.939 1.956 3.676 2.666 5.204 2.666 2.112 0 3.823-1.357 5.11-2.652 2.034-1.93 4.366-4.454 5-7.285l-.015.008c1.113-.726 2.261-1.77 3.538-3.057l2.81-2.872c4.619-4.659 2.226-8.166.013-10.402z",
				},
				new Sample
				{
					Title = "Homies",
					Description = "Regroups your memories and your best friends ones!",
				},
				new Sample
				{
					Title = "Form",
					Description = "Input data in the form",
				},
			};

			var componentsSamples = new Sample[]
			{
				new Sample
				{
					Title = "Animations",
				},
				new Sample
				{
					Title = "Brushes",
				},
				new Sample
				{
					Title = "Button",
				},
				new Sample
				{
					Title = "CheckBox",
				},
				new Sample
				{
					Title = "CommandBar",
				},
				new Sample
				{
					Title = "FlipView",
				},
				new Sample
				{
					Title = "GridView",
				},
				new Sample
				{
					Title = "HyperlinkButton",
				},
				new Sample
				{
					Title = "Image",
				},
				new Sample
				{
					Title = "ListView",
				},
				new Sample
				{
					Title = "MessageDialog",
				},
				new Sample
				{
					Title = "Panels",
				},
				new Sample
				{
					Title = "PasswordBox",
				},
				new Sample
				{
					Title = "Progress",
				},
				new Sample
				{
					Title = "RadioButton",
				},
				new Sample
				{
					Title = "ScrollViewer",
				},
				new Sample
				{
					Title = "Shapes",
				},
				new Sample
				{
					Title = "Slider",
				},
				new Sample
				{
					Title = "TextBlock",
				}
				,new Sample
				{
					Title = "TextBox",
				},
				new Sample
				{
					Title = "ToggleButton",
				},
				new Sample
				{
					Title = "ToggleSwitch",
				},
				new Sample
				{
					Title = "Transform",
				},
				new Sample
				{
					Title = "Typography",
				},
				new Sample
				{
					Title = "WebView",
				},
			};

			var featured = new Grouping<string, Sample>("Featured", featuredSamples);
			var components = new Grouping<string, Sample>("Components", componentsSamples);

#if __WASM__
			DataContext = featured.Concat(components).ToArray();
#else
			var cvs = new CollectionViewSource
			{
				Source = new[] { featured, components },
				IsSourceGrouped = true,
			};

			DataContext = cvs.View;
#endif
		}

#if __IOS__ || __ANDROID__
		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			if (e.SourcePageType == typeof(Samples.Playground))
			{
				Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Portrait;
			}

			base.OnNavigatingFrom(e);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			if (e.NavigationMode == NavigationMode.Back)
			{
				// Force the portrait mode, the reset to none
				Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Portrait;
				Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.None;
			}
		}
#endif

		private void SamplesList_ItemClick(object sender, ItemClickEventArgs e)
		{
			var sample = e.ClickedItem as Sample;
			var type = Type.GetType($"Uno.UI.Demo.Samples.{sample.Title}");

			if (IsPage(type))
			{
				Frame.Navigate(type);
			}
			else if (IsUserControl(type))
			{
				Frame.Navigate(typeof(SamplePage), type);
			}
			else
			{
				throw new Exception();
			}
		}

		private bool IsPage(Type type)
		{
			return typeof(Page).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
		}

		private bool IsUserControl(Type type)
		{
			return !IsPage(type) && typeof(UserControl).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
		}
	}
}
