using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Controllers;
using EasyPasswordRecoveryWiFi.Helpers;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Messages;
using EasyPasswordRecoveryWiFi.Models.Wlan;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class MainViewModel : Screen, IShell
	{
		#region [ Injected instances ]

		private readonly IWindowManager _windowManager = null;
		private readonly IEventAggregator _eventAggregator = null;
		private readonly WiFiSearchViewModel _wiFiSearchViewModel = null;
		private readonly IBusyIndicator _busyIndicator = null;
		private readonly IErrorHandler _errorHandler = null;
		private readonly MainController _mainController = null;
		private readonly PropertiesViewModel _propertiesViewModel = null;
		private readonly PasswordViewModel _passwordViewModel = null;
		private readonly IStorageService _storageService = null;
		private readonly IConfigurationProvider _configurationProvider = null;

		#endregion

		#region [ Constructor ]

		public MainViewModel(IEventAggregator eventAggregator,
			IWindowManager windowManager,
			MainController mainController,
			IErrorHandler errorHandler,
			IBusyIndicator busyIndicator,
			WiFiSearchViewModel wiFiSearchViewModel,
			PropertiesViewModel propertiesViewModel,
			PasswordViewModel passwordViewModel,
			IStorageService storageService,
            IConfigurationProvider configurationProvider,
			IEnumerable<IPasswordProvider> passwordProviders)
		{
			_eventAggregator = eventAggregator;
			_windowManager = windowManager;
			_errorHandler = errorHandler;
			_busyIndicator = busyIndicator;
			_mainController = mainController;
			_wiFiSearchViewModel = wiFiSearchViewModel;
			_propertiesViewModel = propertiesViewModel;
			_passwordViewModel = passwordViewModel;
			_storageService = storageService;
			_configurationProvider = configurationProvider;
			PasswordProviders = new ObservableCollection<IPasswordProvider>(passwordProviders);
			AccessPoints = new ObservableCollection<AccessPoint>();
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
			_busyIndicator.ApplicationState += HandleBusyEvent;
			base.OnActivate();
		}

		/// <summary>
		/// Clean up when leaving this view. 
		/// </summary>
		protected override void OnDeactivate(bool close)
		{
			_busyIndicator.ApplicationState -= HandleBusyEvent;
			base.OnDeactivate(close);
		}

		#endregion

		#region [ Properties ]

		private bool isBusy = false;
		/// <summary>
		/// Busy flag used to enable/disable controls in the view. 
		/// </summary>
		public bool IsBusy
		{
			get { return isBusy; }
			set
			{
				if (Set(ref isBusy, value))
				{
					NotifyOfPropertyChange();
					NotifyOfPropertyChange(nameof(CanCancelCmd));
					NotifyOfPropertyChange(nameof(CanStartCmd));
				}
			}
		}

		private ObservableCollection<AccessPoint> accessPoints = null;
		/// <summary>
		/// Selected access point to connect to using the password match process.
		/// The collection contains only one access point.
		/// An ObservableCollection is used because of the binding with a datagrid.
		/// </summary>
		public ObservableCollection<AccessPoint> AccessPoints
		{
			get { return accessPoints; }
			set
			{
				if (Set(ref accessPoints, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private Interface selectedInterface = null;
		/// <summary>
		/// Selected interface that contains the access point. 
		/// </summary>
		public Interface SelectedInterface
		{
			get { return selectedInterface; }
			set
			{
				selectedInterface = value;
				NotifyOfPropertyChange();
				NotifyOfPropertyChange(nameof(CanDisconnectCmd));
				NotifyOfPropertyChange(nameof(CanPropertiesCmd));
			}
		}

		private AccessPoint selectedAccessPoint = null;
		/// <summary>
		/// Selected access point to connect to using the password match process. 
		/// </summary>
		public AccessPoint SelectedAccessPoint
		{
			get { return selectedAccessPoint; }
			set
			{
				selectedAccessPoint = value;
				NotifyOfPropertyChange();
				NotifyOfPropertyChange(nameof(CanSelectAccessPointCmd));
				NotifyOfPropertyChange(nameof(CanRemoveAccessPointCmd));
				NotifyOfPropertyChange(nameof(CanPropertiesCmd));
				NotifyOfPropertyChange(nameof(CanConnectCmd));
			}
		}

		private ObservableCollection<IPasswordProvider> passwordProviders = null;
		/// <summary>
		/// PasswordProviders, contains two members, dictionary list and a RegEx list.
		/// </summary>
		public ObservableCollection<IPasswordProvider> PasswordProviders
		{
			get { return passwordProviders; }
			set
			{
				if (Set(ref passwordProviders, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private IPasswordProvider selectedPasswordProvider = null;
		/// <summary>
		/// The selected PasswordProvider, either a dictionary list or a RegEx list.  
		/// </summary>
		public IPasswordProvider SelectedPasswordProvider
		{
			get { return selectedPasswordProvider; }
			set
			{
				if (Set(ref selectedPasswordProvider, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private string currentPassword = null;
		/// <summary>
		/// The current password used to connect to the specified access point.      
		/// </summary>
		public string CurrentPassword
		{
			get { return currentPassword; }
			set
			{
				if (Set(ref currentPassword, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		public int passwordCount = 0;
		/// <summary>
		/// The total number of passwords matched.      
		/// </summary>
		public int PasswordCount
		{
			get { return passwordCount; }
			set
			{
				if (Set(ref passwordCount, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private int speed = 0;
		/// <summary>
		/// The number of passwords matched every minute.      
		/// </summary>
		public int Speed
		{
			get { return speed; }
			set
			{
				if (Set(ref speed, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		#endregion

		#region [ Methods ]

		private CancellationTokenSource cts = null;

		/// <summary>
		/// Password match process, uses different passwords (provided by the password provider)
		/// to connect to the selected access point. If a password is found that
		/// gains access to the access point, the password will be saved in file.  
		/// </summary>
		/// <param name="token">CancellationToken to cancel the password match process.</param>
		private async Task MatchPasswords(CancellationToken token)
		{
			string errorMsg = null;
			bool connected = false;
			bool isValid = false;
			string msg = null;

			_busyIndicator.IsBusy = true;
			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info,
				$"Attempting to connect to the specified access point [{SelectedAccessPoint.Ssid}]."));

			string password = SelectedPasswordProvider.GetFirst();
			ReportProgress(password, true);

			while (!token.IsCancellationRequested && password != null && !connected)
			{
				/* Validate the entered password, checks if password complies to password rules. */
				isValid = PasswordHelper.IsValid(password, SelectedAccessPoint.Encryption, ref errorMsg);

				/* If the entered password is valid, try to connect. */
				if (isValid)
				{
					connected = await _mainController.ConnectNetworkAsync(SelectedAccessPoint, password,
						TimeSpan.FromSeconds(_configurationProvider.Timeout), token);
				}
				/* Invalid password, does not comply to wifi password rules. */
				else
				{
					_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info,
						$"{errorMsg}, password=[{password}]."));
					/* Display error message for 400 ms and continue match process. */
					await Task.Delay(400);
					_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info,
						$"Attempting to connect to the specified access point [{SelectedAccessPoint.Ssid}]."));
				}

				/* Get next passwords while not connected. */
				if (!connected)
				{
					password = SelectedPasswordProvider.GetNext();
					if (password != null)
					{
						ReportProgress(password);
					}
				}
			}

			if (connected)
			{
				/* Save password to file, path is configured in SettingsViewModel. */
				FileMode fileMode = _configurationProvider.OverwriteFile ? FileMode.Create : FileMode.Append;
				string filePath = string.Format("{0}\\{1}",
					_configurationProvider.PasswordStorageDir, _configurationProvider.FileName);
				await _storageService.WriteToFileAsync(filePath, fileMode,
					$"Ssid={SelectedAccessPoint.Ssid}, password={password}\n");

				/* If connected, retrieve access point and update status (isConnected) of SelectedAccessPoint. */
				await RefreshAccessPointAsync();
				msg = $"Successfully connected to access point [{SelectedAccessPoint.Ssid}] using password [{password}].";
			}
			else if (token.IsCancellationRequested)
			{
				msg = "Password match process was cancelled.";
			}
			else
			{
				msg = $"Failed to connect to the specified access point [{SelectedAccessPoint.Ssid}].";
			}

			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info, msg));
			_busyIndicator.IsBusy = false;
		}

		private DateTime StartTime = default(DateTime);
		/// <summary>
		/// ReportProgress updates the view about the progress of the password match process.    
		/// </summary>
		private void ReportProgress(string password, bool firstTime = false)
		{
			if (firstTime)
			{
				PasswordCount = 0;
				StartTime = DateTime.Now;
			}
			else
			{
				TimeSpan elapsed = DateTime.Now - StartTime;
				if (elapsed.TotalMilliseconds > 0)
				{
					Speed = (int)(60000 / elapsed.TotalMilliseconds);
				}
				StartTime = DateTime.Now;
			}

			CurrentPassword = password;
			PasswordCount++;
		}

		/// <summary>
		/// Refresh selected access point properties. 
		/// </summary>
		private async Task RefreshAccessPointAsync()
		{
			_busyIndicator.IsBusy = true;
			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info,
				$"Refreshing the status of access point [{SelectedAccessPoint.Ssid}]."));

			IEnumerable<AccessPoint> accessPoints = await _mainController.GetWiFiAccessPointsAsync();
			AccessPoint accessPoint = accessPoints.Where(x => x.Id == SelectedAccessPoint.Id &&
			x.Ssid == SelectedAccessPoint.Ssid &&
			x.ProfileName == SelectedAccessPoint.Ssid).FirstOrDefault();

			/* If access point not found, repeat search without specifying profile name. */
			if (accessPoint == default(AccessPoint))
			{
				accessPoint = accessPoints.Where(x => x.Id == SelectedAccessPoint.Id &&
				x.Ssid == SelectedAccessPoint.Ssid).FirstOrDefault();
			}

			/* Update access point in view. */
			if (accessPoint != default(AccessPoint))
			{
				AccessPoints.Clear();
				AccessPoints.Add(accessPoint);
				SelectedAccessPoint = accessPoint;
			}

			/* Update SelectedInterface status, (isConnected). */
			IEnumerable<Interface> interfaces = await _mainController.GetWiFiInterfacesAsync();
			Interface wiFiInterface = interfaces.Where(x => x.Id == SelectedInterface.Id).FirstOrDefault();
			if (wiFiInterface != default(Interface))
			{
				SelectedInterface = wiFiInterface;
			}

			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.None));
			_busyIndicator.IsBusy = false;
		}

		/// <summary>
		/// Disconnects from the selected interface. 
		/// </summary>
		public async Task DisconnectAsync()
		{
			_busyIndicator.IsBusy = true;
			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info,
				$"Disconnecting from interface [{SelectedInterface.Description}]."));

			await _mainController.DisconnectNetworkAsync(SelectedInterface,
				TimeSpan.FromSeconds(_configurationProvider.Timeout), CancellationToken.None);

			await RefreshAccessPointAsync();

			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.None));
			_busyIndicator.IsBusy = false;
		}

		/// <summary>
		/// Manually connects to the selected access point. 
		/// The user will be prompted for a password.
		/// </summary>
		public async Task ConnectAsync()
		{
			bool dialogResult = false;
			string password = null;
			string msg = null;

			_busyIndicator.IsBusy = true;
			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info,
				$"Connecting to access point [{SelectedAccessPoint.Ssid}]."));

			_passwordViewModel.SelectedAccessPoint = SelectedAccessPoint;
			dialogResult = _windowManager.ShowDialog(_passwordViewModel) ?? false;

			if (dialogResult)
			{
				password = _passwordViewModel.Password;
				bool isConnected = await _mainController.ConnectNetworkAsync(SelectedAccessPoint,
					password, TimeSpan.FromSeconds(_configurationProvider.Timeout), CancellationToken.None);

				if (isConnected)
				{
					msg = $"Successfully connected to access point [{SelectedAccessPoint.Ssid}].";
					await RefreshAccessPointAsync();
				}
				else
				{
					msg = $"Failed to connect to access point [{SelectedAccessPoint.Ssid}].";
				}
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info, msg));
			}
			else
			{
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.None));
			}

			_busyIndicator.IsBusy = false;
		}

		#endregion

		#region [ Command handlers ]

		/// <summary>
		/// Start password match process. In order to start password match process, user must have selected
		/// a access point and configured a password provider.
		/// </summary>
		public void StartCmd()
		{
			if (SelectedAccessPoint == null)
			{
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info,
					"No access point selected."));
			}
			else if (SelectedPasswordProvider == null || SelectedPasswordProvider.IsEmpty)
			{
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info,
					"No password provider configured."));
			}
			else
			{
				cts = new CancellationTokenSource();
				MatchPasswords(cts.Token).FireAndForgetSafeAsync(_errorHandler);
			}
		}

		/// <summary>
		/// If not busy, enable password match process.      
		/// </summary>
		public bool CanStartCmd
		{
			get { return !IsBusy; }
		}

		/// <summary>
		/// Cancel password match process using cancellationtoken.      
		/// </summary>
		public void CancelCmd()
		{
			cts?.Cancel();
		}

		/// <summary>
		/// If busy, enable cancellation of password match process.      
		/// </summary>
		public bool CanCancelCmd
		{
			get { return IsBusy; }
		}

		/// <summary>
		/// Select access point from the access points in range.      
		/// </summary>
		public void SelectAccessPointCmd()
		{
			try
			{
				bool dialogResult = _windowManager.ShowDialog(_wiFiSearchViewModel) ?? false;
				if (dialogResult)
				{
					AccessPoints.Add(_wiFiSearchViewModel.SelectedAccessPoint);
					SelectedAccessPoint = _wiFiSearchViewModel.SelectedAccessPoint;
					SelectedInterface = _wiFiSearchViewModel.SelectedInterface;
				}
			}
			catch (Exception ex)
			{
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Error, ex.Message));
				_busyIndicator.ResetState();
			}
		}

		/// <summary>
		/// If no access point is selected, enable select command.      
		/// </summary>
		public bool CanSelectAccessPointCmd
		{
			get { return SelectedAccessPoint == null; }
		}

		/// <summary>
		/// Remove selected access point.      
		/// </summary>
		public void RemoveAccessPointCmd()
		{
			AccessPoints.Clear();
			SelectedAccessPoint = null;
			SelectedInterface = null;
		}

		/// <summary>
		/// If datagrid in view contains an access point, enable remove command.      
		/// </summary>
		public bool CanRemoveAccessPointCmd
		{
			get { return SelectedAccessPoint != null; }
		}

		/// <summary>
		/// Disconnect from the selected interface.   
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

				if (SelectedInterface != null && SelectedInterface.IsConnected)
				{
					canDisconnect = true;
				}

				return canDisconnect;
			}
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
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Error, ex.Message));
				_busyIndicator.ResetState();
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

				if (SelectedInterface != null && SelectedAccessPoint != null)
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

		#endregion

		#region [ Event handlers ]

		/// <summary>
		/// Busy event used to set the <see cref="IsBusy"/> flag.  
		/// </summary>
		private void HandleBusyEvent(object sender, BusyEventArgs e)
		{
			IsBusy = e.IsBusy;
		}

		#endregion
	}
}
