using System;
using System.Collections.Generic;
using System.Text;
using Uno.UI.Demo.Samples;
using Windows.UI.Xaml.Data;
using Windows.Web.Syndication;

namespace Uno.UI.Demo.Converters
{
	public class BoolToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if(value is bool)
			{
				if ((bool)value)
				{
					return "Is Checked";
				}
				else
				{
					return "Is Not Checked";
				}
			}
			return "Invalid";
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
