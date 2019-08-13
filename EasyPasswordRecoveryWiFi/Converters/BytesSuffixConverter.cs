﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace EasyPasswordRecoveryWiFi.Converters
{
	[ValueConversion(typeof(long), typeof(string))]
	/// <summary>
	/// Converts data size values to it corresponding suffix kilobyte, megabyte, and gigabyte.  
	/// </summary>
	public class BytesSuffixConverter : IValueConverter
	{
		private readonly string[] Suffix = { "B", "KB", "MB", "GB", "TB" };

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			long bytes = (long)value;
			return formatBytes(bytes);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		private string formatBytes(long bytes)
		{
			int index = 0;
			string formatBytes;
			double fileSize = bytes;
			while ((int)((double)fileSize / 1024.0f) > 0)
			{
				fileSize /= 1024.0f;
				index++;
			}

			if (index == 0)
			{
				formatBytes = String.Format("{0} {1}", bytes, Suffix[index]);
			}
			else
			{
				formatBytes = String.Format("{0:0.00} {1}", fileSize, Suffix[index]);
			}

			return formatBytes;
		}
	}
}
