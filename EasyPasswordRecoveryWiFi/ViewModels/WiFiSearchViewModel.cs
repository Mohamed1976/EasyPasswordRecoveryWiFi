using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Controllers;
using EasyPasswordRecoveryWiFi.Helpers;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Models.Wlan;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class WiFiSearchViewModel : Screen, IShell
	{
		#region [ Injected instances ]

		private readonly IEventAggregator _eventAggregator = null;
		private readonly IWindowManager _windowManager = null;
		private readonly MainController _mainController = null;
		private readonly IErrorHandler _errorHandler = null;
		private readonly PropertiesViewModel _propertiesViewModel = null;
		private readonly PasswordViewModel _passwordViewModel = null;
		private readonly IProfileService _profileService = null;
		private readonly IConfigurationProvider _configurationProvider = null;

		#endregion

		#region [ Constructor ]

		public WiFiSearchViewModel(IEventAggregator eventAggregator,
			IWindowManager windowManager,
			MainController mainController,
			IErrorHandler errorHandler,
			PropertiesViewModel propertiesViewModel,
			PasswordViewModel passwordViewModel,
			StatusBarViewModel statusBarViewModel,
			IConfigurationProvider configurationProvider,
			IBusyStateManager busyStateManager,
			IProfileService profileService)
		{
			_eventAggregator = eventAggregator;
			_windowManager = windowManager;
			_mainController = mainController;
			_errorHandler = errorHandler;
			_propertiesViewModel = propertiesViewModel;
			_passwordViewModel = passwordViewModel;
			_configurationProvider = configurationProvider;
			_profileService = profileService;
			StatusBarBottom = statusBarViewModel;
			BusyStateManager = busyStateManager;
            StatusBarBottom.ConductWith(this);
			WiFiAccessPointViewSource = new CollectionViewSource();
			WiFiAccessPointViewSource.Filter += ApplyFilter;
		}

		#endregion

		#region [ Screen overrides ]

		protected override void OnInitialize()
		{
			base.OnInitialize();
		}

		/// <summary>
		/// Initializations when entering this view. 
		/// </summary>
		protected override void OnActivate()
		{
			DownloadAccessPointsAsync().FireAndForgetSafeAsync(_errorHandler);
			base.OnActivate();
		}

		/// <summary>
		/// Clean up when leaving this view. 
		/// </summary>
		protected override void OnDeactivate(bool close)
		{
			/* Clear status messages from current Dialog Window. */
			BusyStateManager.SetMessage(SeverityType.None);
			base.OnDeactivate(close);
		}

        #endregion

        #region [ Properties ]

		public IBusyStateManager BusyStateManager { get; }

		/// <summary>
		/// StatusBar (ViewModel) on bottom of view, displays info/errors to the user.  
		/// </summary>
		public IShell StatusBarBottom { get; }

		private AccessPoint selectedAccessPoint;
		/// <summary>
		/// Selected wifi access point, set in the view (datagrid) and in <see cref="DownloadAccessPointsAsync"/>.  
		/// </summary>
		public AccessPoint SelectedAccessPoint
		{
			get { return selectedAccessPoint; }
			set
			{
				selectedAccessPoint = value;
				NotifyOfPropertyChange();
				NotifyOfPropertyChange(nameof(CanPropertiesCmd));
				NotifyOfPropertyChange(nameof(CanConnectCmd));
				NotifyOfPropertyChange(nameof(CanSelectCmd));
			}
		}

		private Interface selectedInterface = null;
		/// <summary>
		/// The selected interface, selected from interface list in view and in <see cref="DownloadAccessPointsAsync"/>. 
		/// </summary>
		public Interface SelectedInterface
		{
			get { return selectedInterface; }
			set
			{
				selectedInterface = value;
				NotifyOfPropertyChange();
				NotifyOfPropertyChange(nameof(CanPropertiesCmd));
				NotifyOfPropertyChange(nameof(CanDisconnectCmd));
				NotifyOfPropertyChange(nameof(CanSelectCmd));
				WiFiAccessPointViewSource.View.Refresh();
			}
		}

		private ObservableCollection<AccessPoint> accessPoint = null;
		/// <summary>
		/// A list of wifi access points found on all interfaces. 
		/// </summary>
		public ObservableCollection<AccessPoint> AccessPoints
		{
			get { return accessPoint; }
			set
			{
				if (Set(ref accessPoint, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private ObservableCollection<Interface> interfaces = null;
		/// <summary>
		/// A list of wifi interfaces present on device. 
		/// </summary>
		public ObservableCollection<Interface> Interfaces
		{
			get { return interfaces; }
			set
			{
				if (Set(ref interfaces, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private CollectionViewSource wiFiAccessPointViewSource = null;
		/// <summary>
		/// View used to display access points that are filterd according to <see cref="ApplyFilter"/>.  
		/// </summary>
		public CollectionViewSource WiFiAccessPointViewSource
		{
			get { return wiFiAccessPointViewSource; }
			set
			{
				if (Set(ref wiFiAccessPointViewSource, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		#endregion

		#region [ Methods ]

		/// <summary>
		/// Filters the access points according to the selected interface.
		/// </summary>
		private void ApplyFilter(object sender, FilterEventArgs e)
		{
			AccessPoint wiFiAccessPoint = e.Item as AccessPoint;
			if (wiFiAccessPoint != null && SelectedInterface != null)
			{
				e.Accepted = true;
				if (wiFiAccessPoint.Id == SelectedInterface.Id)
				{
					e.Accepted = true;
				}
				else
				{
					e.Accepted = false;
				}
			}
		}

		/// <summary>
		/// Retrieves access points and interfaces from the device and updates the view.  
		/// </summary>
		private async Task DownloadAccessPointsAsync()
		{
			BusyStateManager.EnterBusy();
			BusyStateManager.SetMessage(SeverityType.Info, "Retrieving access points and interfaces.");

			/* Temporary store selected interface and access point so they can be restored after download. */
			Guid? interfaceId = SelectedInterface?.Id;
			string profileName = SelectedAccessPoint?.ProfileName;
			string ssid = SelectedAccessPoint?.Ssid;

			/* Scan and download access points and interfaces. */
			await _mainController.ScanNetworkAsync(TimeSpan.FromSeconds(_configurationProvider.Timeout),
				CancellationToken.None);
			IEnumerable<AccessPoint> accessPoints = await _mainController.GetWiFiAccessPointsAsync();
			IEnumerable<Interface> interfaces = await _mainController.GetWiFiInterfacesAsync();

			if (accessPoints == default(IEnumerable<AccessPoint>) || interfaces == default(IEnumerable<Interface>))
			{
				AccessPoints = new ObservableCollection<AccessPoint>(Enumerable.Empty<AccessPoint>());
				WiFiAccessPointViewSource.Source = AccessPoints;
				Interfaces = new ObservableCollection<Interface>(Enumerable.Empty<Interface>());
			}
			else
			{
				/* User specified threshold is used to filter access points that meet this threshold. */
				AccessPoints = new ObservableCollection<AccessPoint>(accessPoints
					.Where(x => x.LinkQuality > _configurationProvider.Threshold));
				WiFiAccessPointViewSource.Source = AccessPoints;
				Interfaces = new ObservableCollection<Interface>(interfaces);
			}

			/* Restore selected interface and access point. */
			Interface selectedInterface = Interfaces.Where(x => x.Id == interfaceId).FirstOrDefault();
			AccessPoint selectedAccessPoint = default(AccessPoint);
			if (!string.IsNullOrEmpty(profileName))
			{
				selectedAccessPoint = AccessPoints.Where(x => x.Id == interfaceId &&
				x.ProfileName == profileName).FirstOrDefault();
			}
			else if (!string.IsNullOrEmpty(ssid))
			{
				selectedAccessPoint = AccessPoints.Where(x => x.Id == interfaceId &&
				x.Ssid.Equals(ssid, StringComparison.Ordinal)).FirstOrDefault();
			}

			SelectedInterface = selectedInterface == default(Interface) ?
				Interfaces.FirstOrDefault() : selectedInterface;

			if (selectedAccessPoint != default(AccessPoint))
			{
				SelectedAccessPoint = selectedAccessPoint;
			}

			BusyStateManager.SetMessage(SeverityType.None);
			BusyStateManager.ExitBusy();
		}

		/// <summary>
		/// Disconnects from the selected interface. 
		/// </summary>
		public async Task DisconnectAsync()
		{
			BusyStateManager.EnterBusy();
			BusyStateManager.SetMessage(SeverityType.Info, 
				$"Disconnecting from interface [{SelectedInterface.Description}].");

			await _mainController.DisconnectNetworkAsync(SelectedInterface,
				TimeSpan.FromSeconds(_configurationProvider.Timeout), CancellationToken.None);

			await DownloadAccessPointsAsync();

			BusyStateManager.SetMessage(SeverityType.None);
			BusyStateManager.ExitBusy();
		}

		/// <summary>
		/// Manually connects to the selected access point. 
		/// The user will be prompted for a password (if required).
		/// </summary>
		public async Task ConnectAsync()
		{
			bool dialogResult = false;
			string password = null;
			string msg = null;

			BusyStateManager.EnterBusy();
			BusyStateManager.SetMessage(SeverityType.Info,
				$"Connecting to access point [{SelectedAccessPoint.Ssid}].");

			/* Check if profile generation is implemented for Access Point. */
			/* If profile generation is not implemented an Exception will be thrown. */
			_profileService.CreateProfileXml(SelectedAccessPoint, password);
			if (SelectedAccessPoint.IsPasswordRequired)
			{
				_passwordViewModel.SelectedAccessPoint = SelectedAccessPoint;
				dialogResult = _windowManager.ShowDialog(_passwordViewModel) ?? false;
				if (dialogResult)
				{
					password = _passwordViewModel.Password;
				}
			}

			if ((SelectedAccessPoint.IsPasswordRequired && dialogResult) ||
				!SelectedAccessPoint.IsPasswordRequired)
			{
				bool isConnected = await _mainController.ConnectNetworkAsync(SelectedAccessPoint,
					password, TimeSpan.FromSeconds(_configurationProvider.Timeout), CancellationToken.None);

				await DownloadAccessPointsAsync();

				if (isConnected)
				{
					msg = $"Successfully connected to access point [{SelectedAccessPoint.Ssid}].";
				}
				else
				{
					msg = $"Failed to connect to access point [{SelectedAccessPoint.Ssid}].";
				}

				BusyStateManager.SetMessage(SeverityType.Info, msg);
            }

			if (SelectedAccessPoint.IsPasswordRequired && !dialogResult)
			{
				BusyStateManager.SetMessage(SeverityType.None);
			}

			BusyStateManager.ExitBusy();
		}

		#endregion

		#region [ Commands handlers ]

		/// <summary>
		/// Retrieves a list of wifi access points and interfaces.  
		/// </summary>
		public void RefreshViewCmd()
		{
			DownloadAccessPointsAsync().FireAndForgetSafeAsync(_errorHandler);
		}

		/// <summary>
		/// Show properties of selected access point and interface in properties view.  
		/// </summary>
		public void PropertiesCmd()
		{
			try
			{
				_propertiesViewModel.SetAccessPoint(SelectedInterface, SelectedAccessPoint);
				_windowManager.ShowDialog(_propertiesViewModel);
			}
			catch (Exception ex)
			{
				BusyStateManager.SetMessage(SeverityType.Error, ex.Message);
				BusyStateManager.ClearBusy();
			}
		}

		/// <summary>
		/// If both access point and interface are selected enable show properties command.       
		/// </summary>
		public bool CanPropertiesCmd
		{
			get
			{
				bool canShowProperties = false;

				if (SelectedInterface != null &&
					SelectedAccessPoint != null)
				{
					canShowProperties = true;
				}

				return canShowProperties;
			}
		}

		/// <summary>
		/// Connect with the selected access point.   
		/// </summary>
		/// <remarks>
		/// An already connected access point will be disconnected.    
		/// </remarks>
		public void ConnectCmd()
		{
			ConnectAsync().FireAndForgetSafeAsync(_errorHandler);
		}

		/// <summary>
		/// If selected access point is not connected enable connect command.   
		/// </summary>
		public bool CanConnectCmd
		{
			get
			{
				bool canConnect = false;

				if (SelectedAccessPoint != null && !SelectedAccessPoint.IsConnected)
				{
					canConnect = true;
				}

				return canConnect;
			}
		}

		/// <summary>
		/// Disconnect from the selected interface, independent of selected access point.   
		/// </summary>
		public void DisconnectCmd()
		{
			DisconnectAsync().FireAndForgetSafeAsync(_errorHandler);
		}

		/// <summary>
		/// If interface is connected enable disconnect command.   
		/// </summary>
		public bool CanDisconnectCmd
		{
			get
			{
				bool canDisconnect = false;

				if (SelectedInterface != null &&
					SelectedInterface.IsConnected)
				{
					canDisconnect = true;
				}

				return canDisconnect;
			}
		}

		/// <summary>
		/// Cancel selection, close view and return false to parent view. 
		/// </summary>
		public void CancelCmd()
		{
			TryClose(false);
		}

		/// <summary>
		/// The SelectCmd() method checks if profile generation is implemented for selected access point.
		/// If profile generation is not implemented an error will be thrown in the
		/// _profileService.CreateProfileXml() method. If profile generation is implemented,
		/// the current view will be closed and true will be returned to parent view.  
		/// </summary>
		/// <remarks>
		/// Enterprise networks wpa/wpa2 are currently not supported.
		/// </remarks>
		public void SelectCmd()
		{
			try
			{
				_profileService.CreateProfileXml(SelectedAccessPoint, null);

				TryClose(true);
			}
			catch (Exception ex)
			{
				BusyStateManager.SetMessage(SeverityType.Error, ex.Message);
				BusyStateManager.ClearBusy();
			}
		}

		/// <summary>
		/// User can select access point, if the access point is not already connected and 
		/// it requires a password.  
		/// </summary>
		public bool CanSelectCmd
		{
			get
			{
				bool canSelect = false;

				if (SelectedAccessPoint != null &&
					SelectedInterface != null &&
					!SelectedAccessPoint.IsConnected &&
					SelectedAccessPoint.IsPasswordRequired)
				{
					canSelect = true;
				}

				return canSelect;
			}
		}

		#endregion
	}
}
