using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Converters;
using EasyPasswordRecoveryWiFi.Helpers;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Models.Wlan;
using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace EasyPasswordRecoveryWiFi.Services
{
	public class ProfileService : IProfileService
	{
		/// <summary>
		/// Target namespace of profile Xml.
		/// </summary>
		private const string Namespace = @"http://www.microsoft.com/networking/WLAN/profile/v1";

		#region [ Injected instances ]

		IProfileFactory _profileFactory = null;

		#endregion

		#region[ Constructor ]

		public ProfileService(IProfileFactory profileFactory)
		{
			_profileFactory = profileFactory;
		}

		#endregion

		#region [ Create profile method ]

		/// <summary>
		/// Creates connection profile to the specified access point.   
		/// </summary>
		/// <param name="accessPoint">Access point for which to create connection profile.</param>
		/// <param name="password">Password needed to connect to access point, null if no password required.</param>
		public string CreateProfileXml(AccessPoint accessPoint, string password)
		{
			string profile = string.Empty;

			if (accessPoint == null)
				throw new ArgumentNullException(nameof(accessPoint));

			profile = _profileFactory.CreateProfileXml(accessPoint, password);

			return profile;
		}

		#endregion

		#region[ Parse method ]

		/// <summary>
		/// Validates connection profile and retrieves the attributes from the connection profile.   
		/// </summary>
		/// <param name="profileXml">Connection profile to validate.</param>
		/// <param name="profile">Profile object contains attributes retrieved from profile.</param>
		/// <returns>True if profile is valide, false otherwise.</returns>
		public bool Parse(string profileXml, ref Profile profile)
		{
			bool isValid = false;

			if (string.IsNullOrWhiteSpace(profileXml))
				throw new ArgumentNullException(nameof(profileXml));

			XDocument document = XDocument.Parse(profileXml);

			if (!string.Equals(document.Root.Name.NamespaceName, Namespace))
				throw new ArgumentException("The namespace in the Xml does not indicate a WiFi profile.",
					nameof(profileXml));

			string name = document.Elements().First().Elements(XName.Get("name", Namespace)).FirstOrDefault()?.Value;
			XElement ssidElement = document.Descendants(XName.Get("SSID", Namespace)).FirstOrDefault();
			string ssidName = ssidElement?.Descendants(XName.Get("name", Namespace)).FirstOrDefault()?.Value;
			string ssidHex = ssidElement?.Descendants(XName.Get("hex", Namespace)).FirstOrDefault()?.Value;
			string connectionType = document.Descendants(XName.Get("connectionType", Namespace)).FirstOrDefault()?.Value;
			string authentication = document.Descendants(XName.Get("authentication", Namespace)).FirstOrDefault()?.Value;
			string encryption = document.Descendants(XName.Get("encryption", Namespace)).FirstOrDefault()?.Value;

			/* Retrieve password data, optional data. */
			string keyType = document.Descendants(XName.Get("keyType", Namespace)).FirstOrDefault()?.Value;
			XElement keyIsEncryptedElement = document.Descendants(XName.Get("protected", Namespace)).FirstOrDefault();
			string keyMaterial = document.Descendants(XName.Get("keyMaterial", Namespace)).FirstOrDefault()?.Value;

			/* ConnectionMode and autoSwitchElement are optional data. */
			XElement connectionModeElement = document.Descendants(XName.Get("connectionMode", Namespace)).FirstOrDefault();
			XElement autoSwitchElement = document.Descendants(XName.Get("autoSwitch", Namespace)).FirstOrDefault();

			if (!string.IsNullOrEmpty(name) &&
				(!string.IsNullOrEmpty(ssidName) ||
				!string.IsNullOrEmpty(ssidHex)) &&
				!string.IsNullOrEmpty(connectionType) &&
				!string.IsNullOrEmpty(authentication) &&
				!string.IsNullOrEmpty(encryption))
			{
				string ssid = !string.IsNullOrEmpty(ssidHex) ?
					HexConverter.HexToString(ssidHex) : ssidName;
				WiFiBssType wiFiBssType = GetBssType(connectionType);
				WiFiAuthentication wiFiAuthentication = GetAuthentication(authentication);
				WiFiEncryptionType wiFiEncryptionType = GetEncryptionType(encryption);

				if (wiFiBssType != default(WiFiBssType) &&
					wiFiAuthentication != default(WiFiAuthentication) &&
					wiFiEncryptionType != default(WiFiEncryptionType))
				{
					if (!ReferenceEquals(profile, null))
					{
						profile.ProfileName = name;
						profile.Ssid = ssid;
						profile.BssType = wiFiBssType;
						profile.Authentication = wiFiAuthentication;
						profile.Encryption = wiFiEncryptionType;
						profile.KeyType = GetKeyType(keyType);
						profile.KeyIsEncrypted = ((bool?)keyIsEncryptedElement).GetValueOrDefault();
						profile.KeyValue = keyMaterial;
						profile.IsAutoConnectEnabled = IsAutoConnectEnabled(connectionModeElement?.Value);
						profile.IsAutoSwitchEnabled = ((bool?)autoSwitchElement).GetValueOrDefault();
						profile.Xml = profileXml;
					}

					isValid = true;
				}
			}

			return isValid;
		}

		#endregion

		#region [ Profile formatter method ]

		/// <summary>
		/// Formats the XML file.
		/// </summary>
		/// <param name="fileXml">The input XML string.</param>
		/// <param name="settings">The settings for the XML formatting.</param>
		/// <returns>A formatted XML file.</returns>
		public string Format(string fileXml, XmlWriterSettings settings = null)
		{
			string formattedXml = string.Empty;
			/* WiFi XML profile is default ASCII encoded. */
			Encoding encoding = Encoding.ASCII;

			if (string.IsNullOrWhiteSpace(fileXml))
				throw new ArgumentNullException(nameof(fileXml));

			XmlDocument xml = new XmlDocument();
			xml.LoadXml(fileXml);

			/* Retrieve the encoding from the XML header, which is used to configure StringWriter. */
			XmlDeclaration declaration = xml.ChildNodes.OfType<XmlDeclaration>().FirstOrDefault();
			if (!string.IsNullOrEmpty(declaration.Encoding))
			{
				encoding = Encoding.GetEncoding(declaration.Encoding);
			}

			if (settings == null)
			{
				// Modify these settings to format the XML as desired
				settings = new XmlWriterSettings
				{
					Indent = true,
					NewLineOnAttributes = true,
					Encoding = encoding
				};
			}

			using (StringWriterWithEncoding sw = new StringWriterWithEncoding(encoding))
			{
				using (var textWriter = XmlWriter.Create(sw, settings))
				{
					xml.Save(textWriter);
				}
				sw.Flush();
				formattedXml = sw.ToString();
			}

			if (string.IsNullOrEmpty(formattedXml))
			{
				throw new NotImplementedException("Unable to format XML file.");
			}
			else
			{
				return formattedXml;
			}
		}

		#endregion

		#region [ Converters ]

		private WiFiBssType GetBssType(string source)
		{
			WiFiBssType wiFiBssType = default(WiFiBssType);

			if (string.Equals("ESS", source, StringComparison.OrdinalIgnoreCase))
			{
				wiFiBssType = WiFiBssType.Infrastructure;
			}
			else if (string.Equals("IBSS", source, StringComparison.OrdinalIgnoreCase))
			{
				wiFiBssType = WiFiBssType.Independent;
			}

			return wiFiBssType;
		}

		private WiFiAuthentication GetAuthentication(string source)
		{
			WiFiAuthentication wiFiAuthentication = default(WiFiAuthentication);

			switch (source)
			{
				case "open":
					wiFiAuthentication = WiFiAuthentication.Open;
					break;
				case "shared":
					wiFiAuthentication = WiFiAuthentication.Shared;
					break;
				case "WPA":
					wiFiAuthentication = WiFiAuthentication.WPA_Enterprise;
					break;
				case "WPAPSK":
					wiFiAuthentication = WiFiAuthentication.WPA_Personal;
					break;
				case "WPA2":
					wiFiAuthentication = WiFiAuthentication.WPA2_Enterprise;
					break;
				case "WPA2PSK":
					wiFiAuthentication = WiFiAuthentication.WPA2_Personal;
					break;
			}

			return wiFiAuthentication;
		}

		private WiFiEncryptionType GetEncryptionType(string source)
		{
			WiFiEncryptionType wiFiEncryptionType = default(WiFiEncryptionType);

			switch (source)
			{
				case "WEP":
					wiFiEncryptionType = WiFiEncryptionType.WEP;
					break;
				case "TKIP":
					wiFiEncryptionType = WiFiEncryptionType.TKIP;
					break;
				case "AES":
					wiFiEncryptionType = WiFiEncryptionType.AES;
					break;
				case "none":
					wiFiEncryptionType = WiFiEncryptionType.None;
					break;
			}

			return wiFiEncryptionType;
		}

		private WiFiKeyType GetKeyType(string source)
		{
			WiFiKeyType wiFiKeyType = default(WiFiKeyType);

			switch (source)
			{
				case "networkKey":
					wiFiKeyType = WiFiKeyType.NetworkKey;
					break;
				case "passPhrase":
					wiFiKeyType = WiFiKeyType.PassPhrase;
					break;
			}

			return wiFiKeyType;
		}

		private bool IsAutoConnectEnabled(string source)
		{
			bool isAutoConnectEnabled = false;

			if (string.Equals("auto", source, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return isAutoConnectEnabled;
		}

		#endregion
	}
}
