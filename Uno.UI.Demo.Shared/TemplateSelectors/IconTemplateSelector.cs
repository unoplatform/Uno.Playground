using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.UI.Demo.TemplateSelectors
{
	public class IconTemplateSelector : DataTemplateSelector
	{
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
			return item is string name 
				&& Application.Current.Resources.TryGetValue(name + "Icon", out object value) 
				&& value is DataTemplate dataTemplate
				? dataTemplate
				: Application.Current.Resources["DefaultIcon"] as DataTemplate;
		}
	}
}
