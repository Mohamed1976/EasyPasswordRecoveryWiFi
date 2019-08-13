using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Models.Wlan;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Controllers
{
	public class MainController
	{
		#region [ Injected instances ]

		private readonly IWiFiService _wiFiService;

		#endregion

		#region [ Constructor ]

		public MainController(IWiFiService wifiService)
		{
			_wiFiService = wifiService;
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
		public Task<bool> ConnectNetworkAsync(AccessPoint accessPoint, string password,
			TimeSpan timeout, CancellationToken cancellationToken)
		{
			return WorkAsync(() => _wiFiService.ConnectNetworkAsync(accessPoint, password,
				timeout, cancellationToken));
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
			return WorkAsync(() => _wiFiService.DisconnectNetworkAsync(wiFiInterface, timeout,
				cancellationToken));
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
			return WorkAsync(() => _wiFiService.ScanNetworkAsync(timeout, cancellationToken));
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
			return WorkAsync(() => _wiFiService.GetWiFiAccessPointsAsync());
		}

		/// <summary>
		/// Retrieves WiFi profiles from all WiFi interfaces.
		/// </summary>
		/// <returns>A list of WiFi profiles.</returns>
		public Task<IEnumerable<Profile>> GetWiFiProfilesAsync()
		{
			return WorkAsync(() => _wiFiService.GetWiFiProfilesAsync());
		}

		/// <summary>
		/// Retrieves WiFi interfaces and related connection information.
		/// </summary>
		/// <returns>A list of WiFi interfaces and related connection information.</returns>
		public Task<IEnumerable<Interface>> GetWiFiInterfacesAsync()
		{
			return WorkAsync(() => _wiFiService.GetWiFiInterfacesAsync());
		}

		#endregion

		#region [ Profile methods ]

		/// <summary>
		/// Sets the position of the specified WiFi profile to the previous position (higher priority). 
		/// </summary>
		/// <param name="Profile">WiFi Profile.</param>
		/// <returns>True if successfully set, false otherwise.</returns>
		public Task<bool> MoveUpProfileAsync(Profile wiFiProfile)
		{
			return WorkAsync(() => _wiFiService.SetProfilePositionAsync(wiFiProfile, wiFiProfile.Position - 1));
		}

		/// <summary>
		/// Sets the position of the specified WiFi profile to the next position (lower priority). 
		/// </summary>
		/// <param name="Profile">WiFi Profile.</param>
		/// <returns>True if successfully set, false otherwise.</returns>
		public Task<bool> MoveDownProfileAsync(Profile wiFiProfile)
		{
			return WorkAsync(() => _wiFiService.SetProfilePositionAsync(wiFiProfile, wiFiProfile.Position + 1));
		}

		/// <summary>
		/// Sets the position of the specified WiFi profile to the first position (0).
		/// </summary>
		/// <param name="Profile">WiFi Profile.</param>
		/// <returns>True if successfully set, false otherwise.</returns>
		public Task<bool> SetProfileAsDefaultAsync(Profile wiFiProfile)
		{
			return WorkAsync(() => _wiFiService.SetProfilePositionAsync(wiFiProfile, 0));
		}

		/// <summary>
		/// Renames the specified wireless profile.
		/// </summary>
		/// <param name="wiFiProfile">Profile to be renamed.</param>
		/// <param name="newProfileName">New profile name.</param>
		/// <returns>True if successfully renamed, false otherwise.</returns>
		public Task<bool> RenameProfileAsync(Profile wiFiProfile, string newProfileName)
		{
			return WorkAsync(() => _wiFiService.RenameProfileAsync(wiFiProfile, newProfileName));
		}

		/// <summary>
		/// Sets (adds or overwrites) the content of a specified WiFi profile.
		/// </summary>
		/// <param name="wiFiInterface">WiFi Interface to add WiFi profile to.</param>
		/// <param name="profileXml">Profile XML.</param>
		/// <returns>True if successfully set, false otherwise.</returns>
		public Task<bool> AddProfileAsync(Interface wiFiInterface, string profileXml)
		{
			return WorkAsync(() => _wiFiService.SetProfileAsync(wiFiInterface, profileXml));
		}

		/// <summary>
		/// Deletes a specified WiFi profile from its corresponding wireless interface.
		/// </summary>
		/// <param name="Profile">WiFi Profile.</param>
		/// <returns>True if successfully deleted, false otherwise.</returns>
		public Task<bool> DeleteProfileAsync(Profile profile)
		{
			return WorkAsync(() => _wiFiService.DeleteProfileAsync(profile));
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
			return WorkAsync(() => _wiFiService.TurnOnOffInterfaceRadio(wiFiInterface, turnRadioOn));
		}

		#endregion

		#region [ WorkAsync Mutex ]

		/* Instantiate a Singleton of the Semaphore with a value of 1. */
		/* This means that only 1 thread can be granted access at a time. */
		private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

		private async Task<T> WorkAsync<T>(Func<Task<T>> perform)
		{
			bool isEntered = false;

			try
			{
				/* A TimeSpan that represents 0 milliseconds to test the wait handle and return immediately. */
				/* Wait returns true if the current thread successfully entered the SemaphoreSlim; otherwise, false. */
				if (!(isEntered = semaphoreSlim.Wait(TimeSpan.Zero)))
					return default(T);

				return await perform.Invoke();
			}
			finally
			{
				if (isEntered)
				{
					/* When the task is ready, release the semaphore. It is vital to ALWAYS   */
					/* release the semaphore when we are ready, or else we will end up with a */
					/* Semaphore that is forever locked. */
					/* This is why it is important to do the Release within a try...finally clause; */
					/* program execution may crash or take a different path, this way you are guaranteed execution */
					semaphoreSlim.Release();
				}
			}
		}

		#endregion
	}
}
