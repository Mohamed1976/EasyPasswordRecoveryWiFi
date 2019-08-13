using System;
using System.Text;

namespace EasyPasswordRecoveryWiFi.Converters
{
	public static class HexConverter
	{
		/// <summary>
		/// The default encoding used by Hex converter is UTF-8.  
		/// </summary>
		private static Encoding _encoding = Encoding.GetEncoding(65001, // UTF-8 code page
			EncoderFallback.ReplacementFallback,
			DecoderFallback.ExceptionFallback);

		/// <summary>
		/// Converts a Hex string to UTF-8 string.
		/// For example: 57656c636f6d65313233 => Welcome123 
		/// </summary>
		/// <param name="source">Hex string</param>
		/// <returns>UTF-8 string</returns>
		public static string HexToString(string source)
		{
			return _encoding.GetString(ToBytes(source));
		}

		/// <summary>
		/// Converts a byte array to string which represents the byte array in hexadecimal format.
		/// </summary>
		/// <param name="source">Original byte array</param>
		/// <returns>Hexadecimal string</returns>
		public static string ToHexadecimalString(byte[] source) =>
			BitConverter.ToString(source).Replace("-", "");

		public static string ToHexadecimalString(string source) =>
			ToHexadecimalString(_encoding.GetBytes(source));

		/// <summary>
		/// Converts string which represents a byte array in hexadecimal format to the byte array.
		/// </summary>
		/// <param name="source">Hexadecimal string</param>
		/// <returns>Original byte array</returns>
		private static byte[] ToBytes(string source)
		{
			if (string.IsNullOrWhiteSpace(source))
				return null;

			var buff = new byte[source.Length / 2];

			for (int i = 0; i < buff.Length; i++)
			{
				try
				{
					buff[i] = Convert.ToByte(source.Substring(i * 2, 2), 16);
				}
				catch (FormatException)
				{
					break;
				}
			}
			return buff;
		}
	}
}
