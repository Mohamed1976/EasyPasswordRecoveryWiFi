using EasyPasswordRecoveryWiFi.Common;
using System.Text.RegularExpressions;

namespace EasyPasswordRecoveryWiFi.Helpers
{
	public static class PasswordHelper
	{
		/// <summary>
		/// Checks if a password is valid for the specified encryption type.
		/// </summary>
		/// <param name="password">Password to validate.</param>
		/// <param name="encryption">Encryption type used by the WiFi access point.</param>
		/// <param name="errorMsg">Not valid reason.</param>
		/// <returns>True if password complies with encryption rules, false otherwise.</returns>
		public static bool IsValid(string password, WiFiEncryptionType encryption, ref string errorMsg)
		{
			bool isValid = false;

			if (encryption == WiFiEncryptionType.None)
			{
				isValid = true;
			}
			// WEP key is 10, 26 or 40 hex digits long.
			else if (encryption == WiFiEncryptionType.WEP &&
				!string.IsNullOrEmpty(password) &&
				(password.Length == 10 || password.Length == 26 || password.Length == 40) &&
				new Regex("^[0-9A-Fa-f]+$").IsMatch(password))
			{
				isValid = true;
			}
			//WPA2-PSK/WPA-PSK 8 to 63 ASCII characters	
			else if ((encryption == WiFiEncryptionType.AES ||
				encryption == WiFiEncryptionType.TKIP) &&
				!string.IsNullOrEmpty(password) &&
				password.Length >= 8 &&
				password.Length <= 63)
			{
				isValid = true;
			}

			/* If password not valid set error message. */
			if (!isValid && encryption == WiFiEncryptionType.WEP)
			{
				errorMsg = "WEP key is 10, 26 or 40 hex digits long.";
			}
			else if (!isValid && (encryption == WiFiEncryptionType.AES ||
				encryption == WiFiEncryptionType.TKIP))
			{
				errorMsg = "AES/TKIP password is 8 to 63 ASCII characters long.";
			}

			return isValid;
		}
	}
}
