using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Models.Wlan;
using System.Collections.Generic;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class PropertiesViewModel : Screen, IShell
	{
		#region [ enum ]

		public enum DisplayType { None = 0, Profile, AccessPoint };

		#endregion

		#region [ Constructor ]

		public PropertiesViewModel()
		{
		}

		#endregion

		#region [ Screen overrides ]

		protected override void OnInitialize()
		{
			base.OnInitialize();
		}

		/// <summary>
		/// Interface properties are read and added to a dictionary, which is displayed in a datagrid.
		/// Depending on which method is called (SetAccessPoint() or SetProfile), properties of
		/// profile or access point are read and added to a dictionary, which is then also displayed in a datagrid.  
		/// </summary>
		protected override void OnActivate()
		{
			Dictionary<string, string> interfaceInfo = new Dictionary<string, string>();
			Dictionary<string, string> info = new Dictionary<string, string>();

			/* Add interface properties to dictionary. */
			if (SelectedInterface != null)
			{
				interfaceInfo.Add("Interface id", SelectedInterface.Id.ToString());
				interfaceInfo.Add("Description", SelectedInterface.Description);
				interfaceInfo.Add("Is connected", SelectedInterface.IsConnected.ToString());
				interfaceInfo.Add("Is radio on", SelectedInterface.IsRadioOn.ToString());
				interfaceInfo.Add("Profile used", SelectedInterface.ProfileName);
				interfaceInfo.Add("Connection mode", SelectedInterface.ConnectionMode.ToString());
				interfaceInfo.Add("Interface state", SelectedInterface.InterfaceState.ToString());
				InterfaceProperties = interfaceInfo;
			}

			/* Add profile properties to dictionary. */
			if (Display == DisplayType.Profile && SelectedProfile != null)
			{
				info.Add("Name", SelectedProfile.ProfileName);
				info.Add("Ssid", SelectedProfile.Ssid);
				info.Add("Is connected", SelectedProfile.IsConnected.ToString());
				info.Add("Profile type", SelectedProfile.ProfileType.ToString());
				info.Add("Bss type", SelectedProfile.BssType.ToString());
				info.Add("Authentication", SelectedProfile.Authentication.ToString());
				info.Add("Encryption", SelectedProfile.Encryption.ToString());
				info.Add("Key type", SelectedProfile.KeyType.ToString());
				if (SelectedProfile.KeyType != WiFiKeyType.None)
				{
					info.Add("Key is encrypted", SelectedProfile.KeyIsEncrypted.ToString());
					info.Add("Key value", SelectedProfile.KeyValue.ToString());
				}
				info.Add("Is auto connect", SelectedProfile.IsAutoConnectEnabled.ToString());
				info.Add("Is auto switch", SelectedProfile.IsAutoSwitchEnabled.ToString());
				info.Add("Priority position", SelectedProfile.Position.ToString());
				Info = info;
			}

			/* Add access point properties to dictionary. */
			if (Display == DisplayType.AccessPoint && SelectedAccessPoint != null)
			{
				info.Add("Ssid", SelectedAccessPoint.Ssid);
				info.Add("Is connected", SelectedAccessPoint.IsConnected.ToString());
				info.Add("Bss type", SelectedAccessPoint.BssType.ToString());
				info.Add("Authentication", SelectedAccessPoint.Authentication.ToString());
				info.Add("Encryption", SelectedAccessPoint.Encryption.ToString());
				info.Add("Is security enabled", SelectedAccessPoint.IsSecurityEnabled.ToString());
				info.Add("Is password required", SelectedAccessPoint.IsPasswordRequired.ToString());
				info.Add("Has profile", SelectedAccessPoint.HasProfile.ToString());
				if (SelectedAccessPoint.HasProfile)
				{
					info.Add("Associated profile", SelectedAccessPoint.ProfileName.ToString());
				}
				info.Add("Connectable", SelectedAccessPoint.NetworkConnectable.ToString());
				if (!SelectedAccessPoint.NetworkConnectable)
				{
					info.Add("Not connectable reason", SelectedAccessPoint.WlanNotConnectableReason);
				}
				info.Add("Link quality (0-100)", SelectedAccessPoint.LinkQuality.ToString());
				info.Add("Frequency (KHz)", SelectedAccessPoint.Frequency.ToString());
				info.Add("Band (GHz)", SelectedAccessPoint.Band.ToString());
				info.Add("Channel", SelectedAccessPoint.Channel.ToString());
				Info = info;
			}

			base.OnActivate();
		}

		protected override void OnDeactivate(bool close)
		{
			base.OnDeactivate(close);
		}

		#endregion

		#region [ Properties ]

		private Dictionary<string, string> interfaceProperties = null;
		public Dictionary<string, string> InterfaceProperties
		{
			get { return interfaceProperties; }
			private set
			{
				if (Set(ref interfaceProperties, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private Dictionary<string, string> info = null;
		public Dictionary<string, string> Info
		{
			get { return info; }
			private set
			{
				if (Set(ref info, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private DisplayType displayType = DisplayType.None;

		public DisplayType Display
		{
			get { return displayType; }
			private set
			{
				if (Set(ref displayType, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private Interface selectedInterface = null;

		private Interface SelectedInterface
		{
			get { return selectedInterface; }
			set
			{
				Set(ref selectedInterface, value);
			}
		}

		private Profile selectedProfile = null;

		private Profile SelectedProfile
		{
			get { return selectedProfile; }
			set
			{
				Set(ref selectedProfile, value);
			}
		}

		private AccessPoint selectedAccessPoint = null;

		private AccessPoint SelectedAccessPoint
		{
			get { return selectedAccessPoint; }
			set
			{
				Set(ref selectedAccessPoint, value);
			}
		}

		#endregion

		#region [ Methods ]

		/// <summary>
		/// SetAccessPoint is called if you want to display a access point and interface info.
		/// This method is called before the view is activated.   
		/// </summary>
		public void SetAccessPoint(Interface wiFiinterface, AccessPoint accessPoint)
		{
			Display = DisplayType.AccessPoint;
			SelectedInterface = wiFiinterface;
			selectedAccessPoint = accessPoint;
		}

		/// <summary>
		/// SetProfile is called if you want to display a profile and interface info.
		/// This method is called before the view is activated.   
		/// </summary>
		public void SetProfile(Interface wiFiinterface, Profile profile)
		{
			Display = DisplayType.Profile;
			SelectedInterface = wiFiinterface;
			SelectedProfile = profile;
		}

		#endregion

		#region [ Command handlers ]

		/// <summary>
		/// Close view, and return control to parent view.
		/// </summary>
		public void CloseCmd()
		{
			TryClose();
		}

		#endregion
	}
}
