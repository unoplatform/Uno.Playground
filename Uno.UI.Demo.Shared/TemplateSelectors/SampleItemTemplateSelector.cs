using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.UI.Demo.TemplateSelectors
{
	public class SampleItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate DemoSampleItemTemplate { get; set; }

		public DataTemplate ComponentSampleItemTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			return Select(item);
		}

		protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		{
			return Select(item);
		}

		private DataTemplate Select(object item)
		{
			var sampleItem = item as Sample;

			if (sampleItem == null)
			{
				return ComponentSampleItemTemplate;
			}

			if (sampleItem.Description != null)
			{
				return DemoSampleItemTemplate;
			}
			return ComponentSampleItemTemplate;
		}
	}
}
