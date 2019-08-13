using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Converters;
using EasyPasswordRecoveryWiFi.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace EasyPasswordRecoveryWiFi.Models.Wlan
{
	public class ProfileFactory : IProfileFactory
	{
		/// <summary>
		/// Path to embedded resources that contain the profile templates.
		/// </summary>
		private const string profilePath = "EasyPasswordRecoveryWiFi.Resources.ProfileXml";

		public ProfileFactory()
		{
		}

		/// <summary>
		/// Create a valid profile xml according to: http://msdn.microsoft.com/en-us/library/ms707381(v=VS.85).aspx
		/// </summary>
		/// <param name="accessPoint">Access point for which the profile is created.</param>
		/// <param name="password">Password for the access point.</param>
		/// <returns></returns>
		public string CreateProfileXml(AccessPoint accessPoint, string password)
		{
			string profile = string.Empty;
			string template = string.Empty;
			string name = accessPoint.Ssid;
			string hex = HexConverter.ToHexadecimalString(accessPoint.Ssid);

			if (accessPoint.BssType != WiFiBssType.Infrastructure)
				throw new NotSupportedException("Only BssType.Infrastructure is supported by Microsoft.");

			switch (accessPoint.Encryption)
			{
				case WiFiEncryptionType.None:
					if (accessPoint.Authentication == WiFiAuthentication.Open)
					{
						template = GetTemplate("OPEN");
						profile = string.Format(template, name, hex);
					}
					break;
				case WiFiEncryptionType.WEP:
					if (accessPoint.Authentication == WiFiAuthentication.Open)
					{
						template = GetTemplate("WEP");
						profile = string.Format(template, name, hex, password);
					}
					break;
				case WiFiEncryptionType.AES:
					if (accessPoint.Authentication == WiFiAuthentication.WPA2_Personal)
					{
						template = GetTemplate("WPA2-PSK");
						profile = string.Format(template, name, hex, password, accessPoint.Encryption);
					}
					else if (accessPoint.Authentication == WiFiAuthentication.WPA_Personal)
					{
						template = GetTemplate("WPA-PSK");
						profile = string.Format(template, name, hex, password, accessPoint.Encryption);
					}
					break;
				case WiFiEncryptionType.TKIP:
					if (accessPoint.Authentication == WiFiAuthentication.WPA_Personal)
					{
						template = GetTemplate("WPA-PSK");
						profile = string.Format(template, name, hex, password, accessPoint.Encryption);
					}
					else if (accessPoint.Authentication == WiFiAuthentication.WPA2_Personal)
					{
						template = GetTemplate("WPA2-PSK");
						profile = string.Format(template, name, hex, password, accessPoint.Encryption);
					}
					break;
			}

			if (string.IsNullOrEmpty(profile))
			{
				throw new NotImplementedException("Profile generation for access point is not implemented.");
			}
			else
			{
				return profile;
			}
		}

		/// <summary>
		/// String Dictionary used to cache profiles
		/// </summary>
		private Dictionary<string, string> cachedProfiles = new Dictionary<string, string>();

		/// <summary>
		/// Retrieves the template for an wireless connection profile.
		/// </summary>
		private string GetTemplate(string templateName)
		{
			string profile = string.Empty;

			if (cachedProfiles.ContainsKey(templateName))
			{
				cachedProfiles.TryGetValue(templateName, out profile);
			}
			else
			{
				string resourceName = string.Format("{0}.{1}.xml", profilePath, templateName);
				using (StreamReader reader =
					new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)))
				{
					profile = reader.ReadToEnd();
				}
				cachedProfiles.Add(templateName, profile);
			}

			return profile;
		}
	}
}
