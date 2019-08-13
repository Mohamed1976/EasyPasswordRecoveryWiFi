using EasyPasswordRecoveryWiFi.Models.Wlan;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Interfaces
{
	public interface IWiFiService : IDisposable
	{
		#region [ Connect/Disconnect ]

		/// <summary>
		/// Asynchronously attempts to connect to the specified WiFi access point using a generated xml profile.
		/// </summary>
		/// <param name="accessPoint">WiFi access point to connect to.</param>
		/// <param name="password">Password required for authorization.</param>
		/// <param name="timeout">Timeout duration.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>True if successfully connected, false if failed or timed out.</returns>
		Task<bool> ConnectNetworkAsync(AccessPoint accessPoint, string password,
			TimeSpan timeout, CancellationToken cancellationToken);

		/// <summary>
		/// Asynchronously disconnects from the WiFi access point associated with the specified 
		/// wireless interface.
		/// </summary>
		/// <param name="wiFiInterface">WiFi Interface to disconnect from.</param>
		/// <param name="timeout">Timeout duration.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>True if successfully disconnected, false if failed or timed out.</returns>
		Task<bool> DisconnectNetworkAsync(Interface wiFiInterface, TimeSpan timeout,
			CancellationToken cancellationToken);

		#endregion

		#region [ Network methods ] 

		/// <summary>
		/// Asynchronously requests wireless interfaces to scan for available WiFi access points.
		/// </summary>
		/// <param name="timeout">Timeout duration.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns>A list of interface IDs that were successfully scanned.</returns>
		Task<IEnumerable<Guid>> ScanNetworkAsync(TimeSpan timeout, CancellationToken cancellationToken);

		/// <summary>
		/// Retrieves available WiFi access points.
		/// </summary>
		/// <returns>A list of available WiFi access points.</returns>
		/// <remarks>
		/// If multiple profiles are associated with the same access point, there will be multiple entries with
		/// the same ssid.
		/// </remarks>
		Task<IEnumerable<AccessPoint>> GetWiFiAccessPointsAsync();

		/// <summary>
		/// Retrieves WiFi profiles from all WiFi interfaces.
		/// </summary>
		/// <returns>A list of WiFi profiles.</returns>
		Task<IEnumerable<Profile>> GetWiFiProfilesAsync();

		/// <summary>
		/// Retrieves WiFi interfaces and related connection information.
		/// </summary>
		/// <returns>A list of WiFi interfaces and related connection information.</returns>
		Task<IEnumerable<Interface>> GetWiFiInterfacesAsync();

		#endregion

		#region [ Profile methods ]

		/// <summary>
		/// Sets the position of the specified WiFi profile to the specified position.
		/// </summary>
		/// <param name="Profile">WiFi Profile.</param>
		/// <param name="position">The specified position.</param>
		/// <returns>True if successfully set, false otherwise.</returns>
		Task<bool> SetProfilePositionAsync(Profile wiFiProfile, int position);

		/// <summary>
		/// Renames the specified wireless profile.
		/// </summary>
		/// <param name="wiFiProfile">Profile to be renamed.</param>
		/// <param name="newProfileName">New profile name.</param>
		/// <returns>True if successfully renamed, false otherwise.</returns>
		Task<bool> RenameProfileAsync(Profile wiFiProfile, string newProfileName);

		/// <summary>
		/// Sets (adds or overwrites) the content of a specified WiFi profile.
		/// </summary>
		/// <param name="wiFiInterface">WiFi Interface to add WiFi profile to.</param>
		/// <param name="profileXml">Profile XML.</param>
		/// <returns>True if successfully set, false otherwise.</returns>
		Task<bool> SetProfileAsync(Interface wiFiInterface, string profileXml);

		/// <summary>
		/// Deletes a specified WiFi profile from its corresponding WiFi interface.
		/// </summary>
		/// <param name="Profile">WiFi Profile.</param>
		/// <returns>True if successfully deleted, false otherwise.</returns>
		Task<bool> DeleteProfileAsync(Profile profile);

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
		Task<bool> TurnOnOffInterfaceRadio(Interface wiFiInterface, bool turnRadioOn);

		#endregion
	}
}
