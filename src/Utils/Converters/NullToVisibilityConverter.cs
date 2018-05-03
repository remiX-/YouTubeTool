using System;
using System.Windows;
using System.Windows.Data;

namespace YouTubeTool.Utils.Converters
{
	[ValueConversion(typeof(object), typeof(Visibility))]
	public class NullToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value != null ? Visibility.Visible : Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return new NotImplementedException();
			//return (Visibility)value == Visibility.Visible;
		}
	}
}