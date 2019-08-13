using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Interfaces;
using ManagedNativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Models.Wlan
{
	public class NativeWiFiAdapter : NativeWifiPlayer, IWiFiService
	{
		#region[ Injected instances ]

		private readonly IProfileService _profileService = null;

		#endregion

		#region [ Constructor ]

		public NativeWiFiAdapter(IProfileService profileService)
		{
			_profileService = profileService;
		}

		~NativeWiFiAdapter()
		{
			Dispose(false);
		}

		#endregion

		#region [ Dispose ]

		private bool _disposed = false;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				// Free any other managed objects here.
			}

			// Free any unmanaged objects here.

			_disposed = true;

			// Call the base class implementation.
			base.Dispose(disposing);
		}

		#endregion

		#region [ Connect/Disconnect ]

		/// <summary>
		/// Asynchronously attempts to connect to the specified WiFi access point using a generated xml profile.
		/// </summary>
		/// <param name="accessPoint">WiFi access point to connect to.</param>
		/// <param name="password">Password required for authorization.</param>
		/// <param name="timeout">Timeout duration.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>True if successfully connected, false if failed or timed out.</returns>
		/// <remarks>
		/// Only BssType.Infrastructure is supported by Microsoft.
		/// WlanSetProfile, ErrorCode: 1206, ErrorMessage: The network connection profile is corrupted.
		/// ReasonCode: 524301, ReasonMessage: The BSS type is not valid.
		/// https://forums.intel.com/s/question/0D50P0000490AeNSAU/the-bss-type-is-not-valid?language=en_US#400176
		/// </remarks>
		public Task<bool> ConnectNetworkAsync(AccessPoint accessPoint, string password,
			TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (accessPoint == null)
				throw new ArgumentNullException(nameof(accessPoint));

			if (accessPoint.IsPasswordRequired && string.IsNullOrEmpty(password))
				throw new ArgumentNullException(nameof(password));

			if (timeout.TotalSeconds < 0)
				throw new ArgumentOutOfRangeException(nameof(timeout));

			return Task.Run(() =>
			{
				string profile = _profileService.CreateProfileXml(accessPoint, password);
				SetProfile(accessPoint.Id, ProfileType.AllUser, profile, null, true);
				return ConnectNetworkAsync(accessPoint.Id, accessPoint.Ssid, BssType.Infrastructure,
					timeout, cancellationToken);
			});
		}

		/// <summary>
		/// Asynchronously disconnects from the WiFi access point associated with the specified 
		/// wireless interface.
		/// </summary>
		/// <param name="wiFiInterface">WiFi Interface to disconnect from.</param>
		/// <param name="timeout">Timeout duration.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>True if successfully disconnected, false if failed or timed out.</returns>
		public Task<bool> DisconnectNetworkAsync(Interface wiFiInterface, TimeSpan timeout,
			CancellationToken cancellationToken)
		{
			if (wiFiInterface == null)
				throw new ArgumentNullException(nameof(wiFiInterface));

			if (timeout.TotalSeconds < 0)
				throw new ArgumentOutOfRangeException(nameof(timeout));

			return DisconnectNetworkAsync(wiFiInterface.Id, timeout, cancellationToken);
		}

		#endregion

		#region [ Network methods ] 

		/// <summary>
		/// Asynchronously requests wireless interfaces to scan for available WiFi access points.
		/// </summary>
		/// <param name="timeout">Timeout duration.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>A list of interface IDs that were successfully scanned.</returns>
		public Task<IEnumerable<Guid>> ScanNetworkAsync(TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (timeout.TotalSeconds < 0)
				throw new ArgumentNullException(nameof(timeout));

			return ScanNetworksAsync(timeout, cancellationToken);
		}

		/// <summary>
		/// Retrieves available WiFi access points.
		/// </summary>
		/// <returns>A list of available WiFi access points.</returns>
		/// <remarks>
		/// If multiple profiles are associated with the same access point, there will be multiple entries with
		/// the same ssid.
		/// </remarks>
		public Task<IEnumerable<AccessPoint>> GetWiFiAccessPointsAsync()
		{
			return Task.Run(() =>
			{
				IEnumerable<AvailableNetworkGroupPack> accessPoints = EnumerateAvailableNetworkGroups();

				return accessPoints.Select(x => new AccessPoint(
					id: x.Interface.Id,
					ssid: x.Ssid.ToString(),
					bssType: BssTypeConverter(x.BssType),
					isSecurityEnabled: x.IsSecurityEnabled,
					profileName: x.ProfileName,
					networkConnectable: x.NetworkConnectable,
					wlanNotConnectableReason: x.WlanNotConnectableReason,
					authentication: AuthenticationMethodConverter(x.Authentication),
					encryption: EncryptionTypeConverter(x.Encryption),
					isConnected: x.IsConnected,
					linkQuality: x.LinkQuality,
					frequency: x.Frequency,
					band: x.Band,
					channel: x.Channel));
			});
		}

		/// <summary>
		/// Retrieves WiFi profiles from all WiFi interfaces.
		/// </summary>
		/// <returns>A list of WiFi profiles.</returns>
		public Task<IEnumerable<Profile>> GetWiFiProfilesAsync()
		{
			return Task.Run(() =>
			{
				IEnumerable<ProfileRadioPack> profiles = EnumerateProfileRadios();

				return profiles.Select(x => new Profile(
					id: x.Interface.Id,
					isConnected: x.IsConnected,
					profileName: x.Name,
					ssid: x.Document.Ssid.ToString(),
					profileType: ProfileTypeConverter(x.ProfileType),
					bssType: BssTypeConverter(x.Document.BssType),
					authentication: AuthenticationMethodConverter(x.Document.Authentication),
					encryption: EncryptionTypeConverter(x.Document.Encryption),
					keyType: KeyTypeModeConverter(x.Document.KeyType),
					keyIsEncrypted: x.Document.KeyIsEncrypted,
					keyValue: x.Document.KeyValue,
					isAutoConnectEnabled: x.Document.IsAutoConnectEnabled,
					isAutoSwitchEnabled: x.Document.IsAutoSwitchEnabled,
					xml: x.Document.Xml,
					position: x.Position));
			});
		}

		/// <summary>
		/// Retrieves WiFi interfaces and related connection information.
		/// </summary>
		/// <returns>A list of WiFi interfaces and related connection information.</returns>
		public Task<IEnumerable<Interface>> GetWiFiInterfacesAsync()
		{
			return Task.Run(() =>
			{
				IEnumerable<InterfaceConnectionInfo> interfaces = EnumerateInterfaceConnections();

				return interfaces.Select(x => new Interface(
					id: x.Id,
					description: x.Description,
					profileName: x.ProfileName,
					isRadioOn: x.IsRadioOn,
					isConnected: x.IsConnected,
					connectionMode: ConnectionModeConverter(x.ConnectionMode),
					interfaceState: InterfaceStateConverter(x.State)));
			});
		}

		#endregion

		#region [ Profile methods ]

		/// <summary>
		/// Sets the position of the specified WiFi profile to the specified position.
		/// </summary>
		/// <param name="Profile">WiFi Profile.</param>
		/// <param name="position">The specified position.</param>
		/// <returns>True if successfully set, false otherwise.</returns>
		public Task<bool> SetProfilePositionAsync(Profile wiFiProfile, int position)
		{
			if (wiFiProfile == null)
				throw new ArgumentNullException(nameof(wiFiProfile));

			if (position < 0)
				throw new ArgumentOutOfRangeException(nameof(position));

			return Task.Run(() => SetProfilePosition(wiFiProfile.Id, wiFiProfile.ProfileName, position));
		}

		/// <summary>
		/// Renames the specified wireless profile.
		/// </summary>
		/// <param name="wiFiProfile">Profile to be renamed.</param>
		/// <param name="newProfileName">New profile name.</param>
		/// <returns>True if successfully renamed, false otherwise.</returns>
		public Task<bool> RenameProfileAsync(Profile wiFiProfile, string newProfileName)
		{
			if (wiFiProfile == null)
				throw new ArgumentNullException(nameof(wiFiProfile));

			if (string.IsNullOrEmpty(newProfileName))
				throw new ArgumentNullException(nameof(newProfileName));

			return Task.Run(() => RenameProfile(wiFiProfile.Id, wiFiProfile.ProfileName, newProfileName));
		}

		/// <summary>
		/// Sets (adds or overwrites) the content of a specified WiFi profile.
		/// </summary>
		/// <param name="wiFiInterface">WiFi Interface to add WiFi profile to.</param>
		/// <param name="profileXml">Profile XML.</param>
		/// <returns>True if successfully set, false otherwise.</returns>
		public Task<bool> SetProfileAsync(Interface wiFiInterface, string profileXml)
		{
			if (wiFiInterface == null)
				throw new ArgumentNullException(nameof(wiFiInterface));

			if (string.IsNullOrEmpty(profileXml))
				throw new ArgumentNullException(nameof(profileXml));

			return Task.Run(() => SetProfile(wiFiInterface.Id, ProfileType.AllUser, profileXml, null, true));
		}

		/// <summary>
		/// Deletes a specified WiFi profile from its corresponding WiFi interface.
		/// </summary>
		/// <param name="Profile">WiFi Profile.</param>
		/// <returns>True if successfully deleted, false otherwise.</returns>
		public Task<bool> DeleteProfileAsync(Profile profile)
		{
			if (profile == null)
				throw new ArgumentNullException(nameof(profile));

			return Task.Run(() => DeleteProfile(profile.Id, profile.ProfileName));
		}

		#endregion

		#region [ Interface radio ]

		/// <summary>
		/// Turns on/off the radio of a specified wireless interface (software radio state only).
		/// </summary>
		/// <param name="wiFiInterface">WiFi interface.</param>
		/// <returns>True if successfully changed radio state. False if failed.</returns>
		/// <exception cref="UnauthorizedAccessException">
		/// If the user is not logged on as a member of Administrators group.
		/// </exception>
		public Task<bool> TurnOnOffInterfaceRadio(Interface wiFiInterface, bool turnRadioOn)
		{
			if (wiFiInterface == null)
				throw new ArgumentNullException(nameof(wiFiInterface));

			return Task.Run(() =>
			{
				bool bSuccess;

				if (turnRadioOn)
				{
					bSuccess = TurnOnInterfaceRadio(wiFiInterface.Id);
				}
				else
				{
					bSuccess = TurnOffInterfaceRadio(wiFiInterface.Id);
				}

				return bSuccess;
			});
		}

		#endregion

		#region [ Converters ]

		/// <summary>
		/// Converts enum BssType (ManagedNativeWifi) to internal type WiFiBssType.
		/// </summary>
		private WiFiBssType BssTypeConverter(BssType bssType)
		{
			WiFiBssType wiFiBssType = default(WiFiBssType); ;
			switch (bssType)
			{
				case BssType.Infrastructure:
					wiFiBssType = WiFiBssType.Infrastructure;
					break;
				case BssType.Independent:
					wiFiBssType = WiFiBssType.Independent;
					break;
			}

			return wiFiBssType;
		}

		/// <summary>
		/// Converts enum ProfileType (ManagedNativeWifi) to internal type WiFiProfileType.
		/// </summary>
		private WiFiProfileType ProfileTypeConverter(ProfileType profileType)
		{
			WiFiProfileType wiFiProfileType = default(WiFiProfileType);

			switch (profileType)
			{
				case ProfileType.AllUser:
					wiFiProfileType = WiFiProfileType.AllUser;
					break;
				case ProfileType.GroupPolicy:
					wiFiProfileType = WiFiProfileType.GroupPolicy;
					break;
				case ProfileType.PerUser:
					wiFiProfileType = WiFiProfileType.PerUser;
					break;
			}

			return wiFiProfileType;
		}

		/// <summary>
		/// Converts enum AuthenticationMethod (ManagedNativeWifi) to internal type WiFiAuthentication.
		/// </summary>
		private WiFiAuthentication AuthenticationMethodConverter(AuthenticationMethod authenticationMethod)
		{
			WiFiAuthentication wiFiAuthentication = default(WiFiAuthentication);

			switch (authenticationMethod)
			{
				case AuthenticationMethod.Open:
					wiFiAuthentication = WiFiAuthentication.Open;
					break;
				case AuthenticationMethod.Shared:
					wiFiAuthentication = WiFiAuthentication.Shared;
					break;
				case AuthenticationMethod.WPA_Enterprise:
					wiFiAuthentication = WiFiAuthentication.WPA_Enterprise;
					break;
				case AuthenticationMethod.WPA_Personal:
					wiFiAuthentication = WiFiAuthentication.WPA_Personal;
					break;
				case AuthenticationMethod.WPA2_Enterprise:
					wiFiAuthentication = WiFiAuthentication.WPA2_Enterprise;
					break;
				case AuthenticationMethod.WPA2_Personal:
					wiFiAuthentication = WiFiAuthentication.WPA2_Personal;
					break;
			}

			return wiFiAuthentication;
		}

		/// <summary>
		/// Converts enum EncryptionType (ManagedNativeWifi) to internal type WiFiEncryptionType.
		/// </summary>
		private WiFiEncryptionType EncryptionTypeConverter(EncryptionType encryptionType)
		{
			WiFiEncryptionType wiFiEncryptionType = default(WiFiEncryptionType);

			switch (encryptionType)
			{
				case EncryptionType.None:
					wiFiEncryptionType = WiFiEncryptionType.None;
					break;
				case EncryptionType.WEP:
					wiFiEncryptionType = WiFiEncryptionType.WEP;
					break;
				case EncryptionType.TKIP:
					wiFiEncryptionType = WiFiEncryptionType.TKIP;
					break;
				case EncryptionType.AES:
					wiFiEncryptionType = WiFiEncryptionType.AES;
					break;
			}

			return wiFiEncryptionType;
		}

		/// <summary>
		/// Converts enum KeyTypeMode (ManagedNativeWifi) to internal type WiFiKeyType.
		/// </summary>
		private WiFiKeyType KeyTypeModeConverter(KeyTypes keyType)
		{
			WiFiKeyType wiFiKeyType = default(WiFiKeyType);

			switch (keyType)
			{
				case KeyTypes.None:
					wiFiKeyType = WiFiKeyType.None;
					break;
				case KeyTypes.NetworkKey:
					wiFiKeyType = WiFiKeyType.NetworkKey;
					break;
				case KeyTypes.PassPhrase:
					wiFiKeyType = WiFiKeyType.PassPhrase;
					break;
			}

			return wiFiKeyType;
		}

		/// <summary>
		/// Converts enum ConnectionMode (ManagedNativeWifi) to internal type WiFiConnectionMode.
		/// </summary>
		private WiFiConnectionMode ConnectionModeConverter(ConnectionMode connectionMode)
		{
			WiFiConnectionMode wiFiConnectionMode = default(WiFiConnectionMode);

			switch (connectionMode)
			{
				case ConnectionMode.Profile:
					wiFiConnectionMode = WiFiConnectionMode.Profile;
					break;
				case ConnectionMode.TemporaryProfile:
					wiFiConnectionMode = WiFiConnectionMode.TemporaryProfile;
					break;
				case ConnectionMode.DiscoverySecure:
					wiFiConnectionMode = WiFiConnectionMode.DiscoverySecure;
					break;
				case ConnectionMode.DiscoveryUnsecure:
					wiFiConnectionMode = WiFiConnectionMode.DiscoveryUnsecure;
					break;
				case ConnectionMode.Auto:
					wiFiConnectionMode = WiFiConnectionMode.Auto;
					break;
			}

			return wiFiConnectionMode;
		}

		/// <summary>
		/// Converts enum InterfaceState (ManagedNativeWifi) to internal type WiFiInterfaceState.
		/// </summary>
		private WiFiInterfaceState InterfaceStateConverter(InterfaceState interfaceState)
		{
			WiFiInterfaceState wiFiInterfaceState = default(WiFiInterfaceState);

			switch (interfaceState)
			{
				case InterfaceState.NotReady:
					wiFiInterfaceState = WiFiInterfaceState.NotReady;
					break;
				case InterfaceState.Connected:
					wiFiInterfaceState = WiFiInterfaceState.Connected;
					break;
				case InterfaceState.AdHocNetworkFormed:
					wiFiInterfaceState = WiFiInterfaceState.AdHocNetworkFormed;
					break;
				case InterfaceState.Disconnecting:
					wiFiInterfaceState = WiFiInterfaceState.Disconnecting;
					break;
				case InterfaceState.Disconnected:
					wiFiInterfaceState = WiFiInterfaceState.Disconnected;
					break;
				case InterfaceState.Associating:
					wiFiInterfaceState = WiFiInterfaceState.Associating;
					break;
				case InterfaceState.Discovering:
					wiFiInterfaceState = WiFiInterfaceState.Discovering;
					break;
				case InterfaceState.Authenticating:
					wiFiInterfaceState = WiFiInterfaceState.Authenticating;
					break;
			}

			return wiFiInterfaceState;
		}

		#endregion
	}
}
