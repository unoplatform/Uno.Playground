using System;
using Windows.UI.Xaml.Controls;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Uno.UI.Demo.Samples
{
	public sealed partial class ContentDialog : UserControl
	{
		public ContentDialog()
		{
			this.InitializeComponent();
		}

		private async void ShowSignInDialog(object sender, object args)
		{
			var result = await new Uno.Playground.Styles.Controls.ContentDialog_SignIn().ShowAsync();
		}

		private async void ShowRateAppDialog(object sender, object args)
		{
			var result = await new Uno.Playground.Styles.Controls.ContentDialogRateApp().ShowAsync();
		}

		private async void ShowTermsDialog(object sender, object args)
		{
			var result = await new Uno.Playground.Styles.Controls.ContentDialog_TermsAndConditions().ShowAsync();
		}

		private void ContentDialog_PrimaryButtonClick(object sender, ContentDialogButtonClickEventArgs args)
		{

		}

		private void ContentDialog_SecondaryButtonClick(object sender, ContentDialogButtonClickEventArgs args)
		{

		}

		private void ContentDialog_CloseButtonClick(object sender, ContentDialogButtonClickEventArgs args)
		{

		}
	}
}
