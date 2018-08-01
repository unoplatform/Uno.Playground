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
	public sealed partial class ListView : UserControl
	{
		public ListView()
		{
			this.InitializeComponent();
			
			int[] list = { 1,2,3,4,5,6,7,8,9,10};
			var section1 = new Grouping<string, int>("First section", list);
			var section2 = new Grouping<string, int>("Second section", list);
			var section3 = new Grouping<string, int>("Third section", list);
			var section4 = new Grouping<string, int>("Fourth section", list);
			var section5 = new Grouping<string, int>("Fifth section", list);
			var section6 = new Grouping<string, int>("Sixth section", list);
			var section7 = new Grouping<string, int>("Seventh section", list);
			var section8 = new Grouping<string, int>("Eighth section", list);
			var section9 = new Grouping<string, int>("Ninth section", list);
			var section10 = new Grouping<string, int>("Tenth section", list);

#if __WASM__
			DataContext = section1
				.Concat(section2)
				.Concat(section3)
				.Concat(section4)
				.Concat(section5)
				.Concat(section6)
				.Concat(section7)
				.Concat(section8)
				.Concat(section9)
				.Concat(section10)
				.ToArray();
#else
			var cvs = new CollectionViewSource
			{
				Source = new[] { section1, section2, section3, section4, section5, section6, section7, section8, section9, section10},
				IsSourceGrouped = true,
			};

			DataContext = cvs.View;
#endif
		}
	}
}
