using EasyPasswordRecoveryWiFi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Models.Wlan
{
	/// <summary>
	/// Wireless access point information.
	/// </summary>
	public class AccessPoint
	{
		public AccessPoint() { }

		public AccessPoint(Guid id, string ssid, WiFiBssType bssType, bool isSecurityEnabled,
			string profileName, bool networkConnectable, string wlanNotConnectableReason,
			WiFiAuthentication authentication, WiFiEncryptionType encryption,
			bool isConnected, int linkQuality, int frequency, float band, int channel)
		{
			Id = id;
			Ssid = ssid;
			BssType = bssType;
			IsSecurityEnabled = isSecurityEnabled;
			ProfileName = profileName;
			NetworkConnectable = networkConnectable;
			WlanNotConnectableReason = wlanNotConnectableReason;
			Authentication = authentication;
			Encryption = encryption;
			IsConnected = isConnected;
			LinkQuality = linkQuality;
			Frequency = frequency;
			Band = band;
			Channel = channel;
		}

		/// <summary>
		/// Interface ID.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Ssid (maximum 32 bytes).
		/// </summary>
		public string Ssid { get; set; }

		/// <summary>
		/// BSS network type.
		/// </summary>
		public WiFiBssType BssType { get; set; }

		/// <summary>
		/// Whether security is enabled on this network.
		/// </summary>
		public bool IsSecurityEnabled { get; set; }

		/// <summary>
		/// Whether password is required to connect to this network 
		/// </summary>
		public bool IsPasswordRequired
		{
			get
			{
				bool isPasswordRequired = true;

				if (!IsSecurityEnabled || Encryption == WiFiEncryptionType.None)
				{
					isPasswordRequired = false;
				}

				return isPasswordRequired;
			}
		}

		/// <summary>
		/// Returns true if this network has a profile, false otherwise
		/// </summary>
		public bool HasProfile
		{
			get
			{
				bool hasProfile = false;

				if (string.IsNullOrEmpty(ProfileName) == false)
				{
					hasProfile = true;
				}

				return hasProfile;
			}
		}

		/// <summary>
		/// Associated wireless profile name.
		/// </summary>
		public string ProfileName { get; set; }

		/// <summary>
		/// Indicates whether the network is connectable or not.
		/// </summary>
		public bool NetworkConnectable { get; set; }

		/// <summary>
		/// Indicates why a network cannot be connected to. This member is only valid when
		/// <see cref="NetworkConnectable"/> is <c>false</c>.
		/// </summary>
		public string WlanNotConnectableReason { get; set; }

		/// <summary>
		/// Authentication method of associated wireless LAN.
		/// </summary>
		public WiFiAuthentication Authentication { get; set; }

		/// <summary>
		/// Encryption type of associated wireless LAN.
		/// </summary>
		public WiFiEncryptionType Encryption { get; set; }

		/// <summary>
		/// Indicates whether the network is currently connected.
		/// </summary>
		public bool IsConnected { get; set; }

		/// <summary>
		/// Link quality of associated BSS network which is the highest link quality.
		/// </summary>
		public int LinkQuality { get; set; }

		/// <summary>
		/// Frequency (KHz) of associated BSS network which has the highest link quality.
		/// </summary>
		public int Frequency { get; set; }

		/// <summary>
		/// Frequency band (GHz) of associated BSS network which has the highest link quality.
		/// </summary>
		public float Band { get; set; }

		/// <summary>
		/// Channel of associated BSS network which has the highest link quality.
		/// </summary>
		public int Channel { get; set; }
	}
}
