using System;
using System.Globalization;
using System.Windows.Data;

namespace EasyPasswordRecoveryWiFi.Converters
{
	[ValueConversion(typeof(bool), typeof(Enum))]
	/// <summary>
	/// Compares the binding value with the passed parameter value, if they are equal,
	/// true is returned, false otherwise. This converter is used to bind radio group buttons to enum values.
	/// </summary>
	public class EnumToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return false;
			else
				return value.Equals(parameter);
		}

		public object ConvertBack(object value,
			Type targetTypes, object parameter, CultureInfo culture)
		{
			if (value == null || parameter == null)
				return false;
			else if ((bool)value)
				return parameter;
			else
				return Binding.DoNothing;
		}
	}
}
