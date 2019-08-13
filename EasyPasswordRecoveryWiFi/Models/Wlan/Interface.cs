using EasyPasswordRecoveryWiFi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Models.Wlan
{
	/// <summary>
	/// Wireless interface information.
	/// </summary>
	public class Interface
	{
		public Interface(Guid id, string description, string profileName, bool isRadioOn,
			bool isConnected, WiFiConnectionMode connectionMode, WiFiInterfaceState interfaceState)
		{
			Id = id;
			Description = description;
			ProfileName = profileName;
			IsRadioOn = isRadioOn;
			IsConnected = isConnected;
			ConnectionMode = connectionMode;
			InterfaceState = interfaceState;
		}

		/// <summary>
		/// Interface ID.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Interface description.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Wireless profile name when a wireless profile is used for the connection.
		/// </summary>
		public string ProfileName { get; set; }

		/// <summary>
		/// Whether the radio of the wireless interface is on.
		/// </summary>
		public bool IsRadioOn { get; set; }

		/// <summary>
		/// Whether the wireless interface is connected to a wireless LAN.
		/// </summary>
		public bool IsConnected { get; set; }

		/// <summary>
		/// Connection mode.
		/// </summary>
		public WiFiConnectionMode ConnectionMode { get; set; }

		/// <summary>
		/// Wireless interface state.
		/// </summary>
		public WiFiInterfaceState InterfaceState { get; set; }
	}
}
