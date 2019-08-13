using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Common
{
	#region [ Enums ]

	/// <summary>
	/// String casing conversions of the dictionaries.
	/// </summary>
	public enum StringCasing
	{
		/// <summary>
		/// None, valid value.
		/// </summary>
		None = 0,

		/// <summary>
		/// Convert strings in dictionaries to lowercase.
		/// </summary>
		LowerCase,

		/// <summary>
		/// Convert strings in dictionaries to uppercase.
		/// </summary>
		UpperCase,

		/// <summary>
		/// Convert strings in dictionaries to titlecase.
		/// </summary>
		TitleCase
	}

	/// <summary>
	/// Describes the severity of the message sent in <see cref="Messages.StatusMsg"/>.
	/// </summary>
	public enum SeverityType
	{
		/// <summary>
		/// None, valid value.
		/// </summary>
		None = 0,

		/// <summary>
		/// Display info message.
		/// </summary>
		Info,

		/// <summary>
		/// Display warning message.
		/// </summary>
		Warning,

		/// <summary>
		/// Display error message.
		/// </summary>
		Error,
	}

	/// <summary>
	/// Describes the kinds of Wi-Fi networks.
	/// </summary>
	public enum WiFiBssType
	{
		/// <summary>
		/// Invalid value.
		/// </summary>
		Invalid = 0,

		/// <summary>
		/// Specifies an infrastructure BSS network.
		/// </summary>
		Infrastructure = 1,

		/// <summary>
		/// Specifies an independent BSS (IBSS) network.
		/// </summary>
		Independent = 2,

		/// <summary>
		/// Specifies either infrastructure or IBSS network.
		/// </summary>
		Any = 3
	}

	/// <summary>
	/// Wireless profile type
	/// </summary>
	public enum WiFiProfileType
	{
		/// <summary>
		/// Invalid value.
		/// </summary>
		Invalid = 0,

		/// <summary>
		/// All-user profile
		/// </summary>
		AllUser,

		/// <summary>
		/// Group policy profile
		/// </summary>
		/// <remarks>Equivalent to WLAN_PROFILE_GROUP_POLICY</remarks>
		GroupPolicy,

		/// <summary>
		/// Per-user profile
		/// </summary>
		/// <remarks>Equivalent to WLAN_PROFILE_USER</remarks>
		PerUser
	}

	/// <summary>
	/// Authentication method to be used to connect to wireless LAN
	/// </summary>
	/// <remarks>
	/// https://msdn.microsoft.com/en-us/library/windows/desktop/ms706933.aspx
	/// </remarks>
	public enum WiFiAuthentication
	{
		/// <summary>
		/// Invalid value.
		/// </summary>
		Invalid = 0,

		/// <summary>
		/// Open 802.11 authentication
		/// </summary>
		Open,

		/// <summary>
		/// Shared 802.11 authentication
		/// </summary>
		Shared,

		/// <summary>
		/// WPA-Enterprise 802.11 authentication
		/// </summary>
		/// <remarks>WPA in profile XML</remarks>
		WPA_Enterprise,

		/// <summary>
		/// WPA-Personal 802.11 authentication
		/// </summary>
		/// <remarks>WPAPSK in profile XML</remarks>
		WPA_Personal,

		/// <summary>
		/// WPA2-Enterprise 802.11 authentication
		/// </summary>
		/// <remarks>WPA2 in profile XML</remarks>
		WPA2_Enterprise,

		/// <summary>
		/// WPA2-Personal 802.11 authentication
		/// </summary>
		/// <remarks>WPA2PSK in profile XML</remarks>
		WPA2_Personal
	}

	/// <summary>
	/// Data encryption type to be used to connect to wireless LAN
	/// </summary>
	/// <remarks>
	/// https://msdn.microsoft.com/en-us/library/windows/desktop/ms706969.aspx
	/// </remarks>
	public enum WiFiEncryptionType
	{
		/// <summary>
		/// Invalid value.
		/// </summary>
		Invalid = 0,

		/// <summary>
		/// None (valid value)
		/// </summary>
		None,

		/// <summary>
		/// WEP encryption for WEP
		/// </summary>
		WEP,

		/// <summary>
		/// TKIP encryption for WPA/WPA2
		/// </summary>
		TKIP,

		/// <summary>
		/// AES (CCMP) encryption for WPA/WPA2
		/// </summary>
		AES
	}

	/// <summary>
	/// The sharedKey (security) element contains shared key information.
	/// This element is only required if WEP or PSK keys are required for the authentication and encryption pair.
	/// </summary>
	/// <remarks>
	/// https://docs.microsoft.com/en-us/windows/desktop/nativewifi/wlan-profileschema-sharedkey-security-element
	/// </remarks>
	public enum WiFiKeyType
	{
		/// <summary>
		/// None, valid value.
		/// </summary>
		None,

		/// <summary>
		/// Shared key will be a network key
		/// </summary>
		NetworkKey,

		/// <summary>
		/// Shared key will be a pass phrase
		/// </summary>
		PassPhrase
	}


	/// <summary>
	/// Connection mode
	/// </summary>
	public enum WiFiConnectionMode
	{
		/// <summary>
		/// Invalid value.
		/// </summary>
		Invalid = 0,

		/// <summary>
		/// A profile will be used to make the connection.
		/// </summary>
		Profile,

		/// <summary>
		/// A temporary profile will be used to make the connection.
		/// </summary>
		TemporaryProfile,

		/// <summary>
		/// Secure discovery will be used to make the connection.
		/// </summary>
		DiscoverySecure,

		/// <summary>
		/// Unsecure discovery will be used to make the connection.
		/// </summary>
		DiscoveryUnsecure,

		/// <summary>
		/// The connection is initiated by the wireless service automatically using a persistent profile.
		/// </summary>
		Auto
	}


	/// <summary>
	/// Wireless interface state
	/// </summary>
	/// <remarks>Equivalent to WLAN_INTERFACE_STATE</remarks>
	public enum WiFiInterfaceState
	{
		/// <summary>
		/// Invalid value.
		/// </summary>
		Invalid = 0,

		/// <summary>
		/// The interface is not ready to operate.
		/// </summary>
		NotReady,

		/// <summary>
		/// The interface is connected to a network.
		/// </summary>
		Connected,

		/// <summary>
		/// The interface is the first node in an ad hoc network. No peer has connected.
		/// </summary>
		AdHocNetworkFormed,

		/// <summary>
		/// The interface is disconnecting from the current network.
		/// </summary>
		Disconnecting,

		/// <summary>
		/// The interface is not connected to any network.
		/// </summary>
		Disconnected,

		/// <summary>
		/// The interface is attempting to associate with a network.
		/// </summary>
		Associating,

		/// <summary>
		/// Auto configuration is discovering the settings for the network.
		/// </summary>
		Discovering,

		/// <summary>
		/// The interface is in the process of authenticating.
		/// </summary>
		Authenticating
	}

	#endregion
}

