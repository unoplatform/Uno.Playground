using System;
using System.Collections.Generic;
using System.Text;
using Uno.Extensions;
using Windows.UI.Xaml.Data;

namespace Uno.UI.Demo.Converters
{
	public class EmptyStringToBoolConverter : IValueConverter
	{
		public bool EmptyStringMeansFalse { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
			{
				return !EmptyStringMeansFalse;
			}

			var text = (string)value;
			return text.IsNullOrEmpty() ^ EmptyStringMeansFalse;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
