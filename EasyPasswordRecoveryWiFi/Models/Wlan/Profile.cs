using EasyPasswordRecoveryWiFi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Models.Wlan
{
	/// <summary>
	/// Wireless connection profile information.
	/// </summary>
	public class Profile
	{
		public Profile() { }

		public Profile(Guid id, bool isConnected, string profileName, string ssid,
			WiFiProfileType profileType, WiFiBssType bssType, WiFiAuthentication authentication,
			WiFiEncryptionType encryption, WiFiKeyType keyType, bool keyIsEncrypted, string keyValue,
			bool isAutoConnectEnabled, bool isAutoSwitchEnabled, string xml, int position)
		{
			Id = id;
			IsConnected = isConnected;
			ProfileName = profileName;
			Ssid = ssid;
			ProfileType = profileType;
			BssType = bssType;
			Authentication = authentication;
			Encryption = encryption;
			KeyType = keyType;
			KeyIsEncrypted = keyIsEncrypted;
			KeyValue = keyValue;
			IsAutoConnectEnabled = isAutoConnectEnabled;
			IsAutoSwitchEnabled = isAutoSwitchEnabled;
			Xml = xml;
			Position = position;
		}

		/// <summary>
		/// Interface ID.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Whether associated wireless interface is connected to associated wireless LAN.
		/// </summary>
		public bool IsConnected { get; set; }

		/// <summary>
		/// Profile name.
		/// </summary>
		public string ProfileName { get; set; }

		/// <summary>
		/// SSID of associated wireless LAN.
		/// </summary>
		public string Ssid { get; set; }

		/// <summary>
		/// Profile type.
		/// </summary>
		public WiFiProfileType ProfileType { get; set; }

		/// <summary>
		/// BSS network type of associated wireless LAN.
		/// </summary>
		public WiFiBssType BssType { get; set; }

		/// <summary>
		/// Authentication of associated wireless LAN.
		/// </summary>
		public WiFiAuthentication Authentication { get; set; }

		/// <summary>
		/// Encryption of associated wireless LAN.
		/// </summary>
		public WiFiEncryptionType Encryption { get; set; }

		/// <summary>
		/// KeyType of profile None, networkKey or passPhrase.
		/// </summary>
		public WiFiKeyType KeyType { get; set; }

		/// <summary>
		/// Returns whether the key is encrypted.
		/// </summary>
		public bool KeyIsEncrypted { get; set; }

		/// <summary>
		/// Returns the network key or passphrase as a string.
		/// </summary>
		public string KeyValue { get; set; }

		/// <summary>
		/// Whether automatic connection for this profile is enabled.
		/// </summary>
		public bool IsAutoConnectEnabled { get; set; }

		/// <summary>
		/// Whether automatic switch for this profile is enabled.
		/// </summary>
		public bool IsAutoSwitchEnabled { get; set; }

		/// <summary>
		/// Profile XML.
		/// </summary>
		public string Xml { get; set; }

		/// <summary>
		/// Position in preference order of associated wireless interface.
		/// </summary>
		public int Position { get; set; }
	}
}
