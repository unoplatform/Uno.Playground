using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.UI.Demo.TemplateSelectors
{
	public class IconTemplateSelector : DataTemplateSelector
	{
		private readonly Dictionary<string, DataTemplate> _cache = new Dictionary<string, DataTemplate>();

		public DataTemplate Default { get; set; }

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
			if (!(item is string name))
			{
				return Default;
			}

			if (_cache.TryGetValue(name, out var cached))
			{
				return cached;
			}

			if (Application.Current.Resources.TryGetValue(name + "Icon", out object value)
				&& value is DataTemplate dataTemplate)
			{
				_cache[name] = dataTemplate;

				return dataTemplate;
			}
			else
			{
				_cache[name] = Default;

				return Default;
			}
		}
	}
}
