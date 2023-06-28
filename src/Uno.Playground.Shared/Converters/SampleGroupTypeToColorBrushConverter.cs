using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.UI.Xaml.Data;

namespace Uno.UI.Demo.Converters
{
	public class SampleGroupTypeToColorBrushConverter : IValueConverter
	{
		public object FeaturedTypeColorBrush { get; set; }
		public object ComponentTypeColorBrush { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null || System.Convert.ToString(value, CultureInfo.InvariantCulture) == "Featured")
			{
				return FeaturedTypeColorBrush;
			}
			else
			{
				return ComponentTypeColorBrush;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
