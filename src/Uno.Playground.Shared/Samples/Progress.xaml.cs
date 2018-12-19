using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Uno.UI.Demo.Samples
{
	public sealed partial class Progress : UserControl
	{
		private int _ticks = 0;
		private DispatcherTimer _timer;

		public Progress()
		{
			this.InitializeComponent();

			Loaded += OnLoaded;
			Unloaded += OnUnloaded;

			_timer = new DispatcherTimer();
			_timer.Interval = TimeSpan.FromMilliseconds(20);
		}

		void OnTick(object s2, object a2)
		{
			_ticks++;
			DataContext = _ticks % 100;
		}

		private void OnLoaded(object sender, RoutedEventArgs args)
		{
			_timer.Tick += OnTick;
			_timer.Start();
		}

		private void OnUnloaded(object sender, RoutedEventArgs args)
		{
			_timer.Tick -= OnTick;
			_timer.Stop();
		}
	}
}
