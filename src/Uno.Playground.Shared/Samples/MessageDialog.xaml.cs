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
	public sealed partial class MessageDialog : UserControl
	{
		public MessageDialog()
		{
			this.InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var messageDialog = new Windows.UI.Popups.MessageDialog("Hello world!");
			messageDialog.ShowAsync();
		}

		private void Button_Click2(object sender, RoutedEventArgs e)
		{
			var messageDialog = new Windows.UI.Popups.MessageDialog("This is a very important message.", "Notice");
			messageDialog.ShowAsync();
		}

		private void Button_Click3(object sender, RoutedEventArgs e)
		{
			var messageDialog = new Windows.UI.Popups.MessageDialog("What is the answer to Life, The Universe and Everything?", "Ultimate Question")
			{
				//CancelCommandIndex = 0,
				Commands =
				{
					new Windows.UI.Popups.UICommand("42"),
					new Windows.UI.Popups.UICommand("Not 42"),
				}
			};
			messageDialog.ShowAsync();
		}
	}
}
