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
    public sealed partial class FlipView : UserControl
    {
        public FlipView()
        {
            this.InitializeComponent();

			// TODO: Workaround because FlipView doesn't stretch vertically on Android
#if __ANDROID__
			void SyncSize()
			{
				flipView.Height = ActualHeight;
				flipView.Width = ActualWidth;
			}

			SizeChanged += (s, e) => SyncSize();
			SyncSize();
#endif
        }

		private void Next(object sender, RoutedEventArgs e)
		{
			flipView.SelectedIndex = (flipView.SelectedIndex + 1) % flipView.Items.Count;
		}
	}
}
