using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.UI.Xaml.Data;

namespace Uno.UI.Demo.Converters
{
	public class IntToMod4DefaultValueConverter : IValueConverter
	{
		public object Mod1Value { get; set; }
		public object Mod2Value { get; set; }
		public object Mod3Value { get; set; }
		public object Mod0Value { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value is int)
			{
				switch (((int)value) % 4)
				{
					case 0: return Mod0Value;
					case 1: return Mod1Value;
					case 2: return Mod2Value;
					case 3: return Mod3Value;
					default: return Mod0Value;
				}
			}
			else
			{
				return Mod0Value;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
