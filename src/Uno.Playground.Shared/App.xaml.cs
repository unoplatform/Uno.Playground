using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Uno.UI.Demo
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		private Frame _rootFrame;

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
#if DEBUG
			ConfigureFilters(LogExtensionPoint.AmbientLoggerFactory);
#endif

			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		static void ConfigureFilters(ILoggerFactory factory)
		{
			factory
				.WithFilter(new FilterLoggerSettings
					{
						{ "Uno", LogLevel.Warning },
						{ "Windows", LogLevel.Warning },
						
						// Generic Xaml events
						//{ "Windows.UI.Xaml", LogLevel.Debug },
						// { "Windows.UI.Xaml.Shapes", LogLevel.Debug },
						//{ "Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug },
						//{ "Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug },
						// { "Windows.UI.Xaml.UIElement", LogLevel.Debug },
						// { "Windows.UI.Xaml.Setter", LogLevel.Debug },
						   
						// Layouter specific messages
						// { "Windows.UI.Xaml.Controls", LogLevel.Debug },
						//{ "Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug },
						//{ "Windows.UI.Xaml.Controls.Panel", LogLevel.Debug },
						   
						// Binding related messages
						// { "Windows.UI.Xaml.Data", LogLevel.Debug },
						// { "Windows.UI.Xaml.Data", LogLevel.Debug },
						   
						//  Binder memory references tracking
						// { "ReferenceHolder", LogLevel.Debug },
					}
				)
#if DEBUG
				.AddConsole(LogLevel.Trace);
#else
				.AddConsole(LogLevel.Debug);
#endif
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			SetupAppCenter();

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif

			_rootFrame = Windows.UI.Xaml.Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (_rootFrame == null)
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				_rootFrame = new Frame()
				{
#if __ANDROID__
					// Workaround to hide splash screen (window background) on Android during navigation
					Background = new SolidColorBrush(Colors.White)
#endif
				};

				_rootFrame.NavigationFailed += OnNavigationFailed;
				_rootFrame.Navigated += RootFrame_Navigated;
				SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}

				// Place the frame in the current Window
				Windows.UI.Xaml.Window.Current.Content = _rootFrame;
			}

			if (e.PrelaunchActivated == false)
			{
				if (_rootFrame.Content == null)
				{
					// When the navigation stack isn't restored navigate to the first page,
					// configuring the new page by passing required information as a navigation
					// parameter
					_rootFrame.Navigate(typeof(SamplesPage), e.Arguments);
				}

				// Ensure the current window is active
				Windows.UI.Xaml.Window.Current.Activate();
			}
		}

		private void App_BackRequested(object sender, BackRequestedEventArgs e)
		{
			if (_rootFrame.CanGoBack)
			{
				e.Handled = true;
				_rootFrame.GoBack();
			}
		}

		private void RootFrame_Navigated(object sender, NavigationEventArgs e)
		{
			SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = _rootFrame.BackStack.Any()
				? AppViewBackButtonVisibility.Visible
				: AppViewBackButtonVisibility.Collapsed;

			Analytics.ReportPageView(e);
		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception($"Failed to load Page {e.SourcePageType.FullName} {e.Exception}");
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e)
		{
			//var deferral = e.SuspendingOperation.GetDeferral();
			////TODO: Save application state and stop any background activity
			//deferral.Complete();
		}

		private void SetupAppCenter()
		{
#if __IOS__ || __ANDROID__ || NETFX_CORE

			var secret =
#if __IOS__
				"190c7b7f-244c-4d05-85b4-5bdcfe332d6f"
#elif __ANDROID__
				"9eb84f65-1c0e-4f59-b5c4-4749cb4b148f"
#elif NETFX_CORE
				"2b40f50e-0047-41a5-9ca0-879568df0f1e"
#endif
			;

			Microsoft.AppCenter.AppCenter.Start(
				secret,
				 typeof(Microsoft.AppCenter.Analytics.Analytics),
				 typeof(Microsoft.AppCenter.Crashes.Crashes)
			);
#endif
		}
	}
}
