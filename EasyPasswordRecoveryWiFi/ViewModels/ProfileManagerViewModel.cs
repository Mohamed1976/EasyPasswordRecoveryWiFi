using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Controllers;
using EasyPasswordRecoveryWiFi.Helpers;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Models.Wlan;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class ProfileManagerViewModel : Screen, IShell
	{
		#region [ Injected instances ]

		private readonly MainController _mainController = null;
		private readonly IEventAggregator _eventAggregator = null;
		private readonly IErrorHandler _errorHandler = null;
		private readonly IProfileService _profileService = null;
		private readonly IWindowManager _windowManager = null;
		private readonly PropertiesViewModel _propertiesViewModel = null;
		private readonly IConfigurationProvider _configurationProvider = null;

		#endregion

		#region [ Constructor ]

		public ProfileManagerViewModel(MainController mainController,
			IEventAggregator eventAggregator,
			IWindowManager windowManager,
			IErrorHandler errorHandler,
			IProfileService profileService,
			IBusyStateManager busyStateManager,
			PropertiesViewModel propertiesViewModel,
			IConfigurationProvider configurationProvider)
		{
			_mainController = mainController;
			_eventAggregator = eventAggregator;
			_errorHandler = errorHandler;
			_profileService = profileService;
			BusyStateManager = busyStateManager;
            _windowManager = windowManager;
			_propertiesViewModel = propertiesViewModel;
			_configurationProvider = configurationProvider;
			WiFiProfilesViewSource = new CollectionViewSource();
			WiFiProfilesViewSource.Filter += ApplyFilter;
		}

		#endregion

		#region [ Screen overrides ]

		protected override void OnInitialize()
		{
			base.OnInitialize();
		}

		/// <summary>
		/// Load the WiFi profiles and interfaces when view is activated. 
		/// </summary>
		protected override void OnActivate()
		{
			DownloadProfilesAsync().FireAndForgetSafeAsync(_errorHandler);
			base.OnActivate();
		}

		protected override void OnDeactivate(bool close)
		{
			base.OnDeactivate(close);
		}

        #endregion

        #region [ Properties ]

		public IBusyStateManager BusyStateManager { get; }

		private ObservableCollection<Profile> wiFiProfiles;
		/// <summary>
		/// A list of WiFi profiles present on interfaces. 
		/// </summary>
		public ObservableCollection<Profile> WiFiProfiles
		{
			get { return wiFiProfiles; }
			set
			{
				if (Set(ref wiFiProfiles, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private ObservableCollection<Interface> interfaces;
		/// <summary>
		/// A list of WiFi interfaces present on device. 
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

		private Interface selectedInterface;
		/// <summary>
		/// The selected interface, selected from interface list. 
		/// </summary>
		public Interface SelectedInterface
		{
			get { return selectedInterface; }
			set
			{
				selectedInterface = value;
				NotifyOfPropertyChange();
				NotifyOfPropertyChange(nameof(CanImportProfileCmd));
				NotifyOfPropertyChange(nameof(CanProfilePropertiesCmd));
				WiFiProfilesViewSource.View.Refresh();
			}
		}

		private CollectionViewSource wiFiProfilesViewSource;
		/// <summary>
		/// View used to display profiles that are filterd according to <see cref="ApplyFilter"/>.  
		/// </summary>
		public CollectionViewSource WiFiProfilesViewSource
		{
			get { return wiFiProfilesViewSource; }
			set
			{
				if (Set(ref wiFiProfilesViewSource, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private Profile selectedWiFiProfile;
		/// <summary>
		/// Selected WiFi profile, set in the View.  
		/// </summary>
		public Profile SelectedWiFiProfile
		{
			get { return selectedWiFiProfile; }
			set
			{
				selectedWiFiProfile = value;
				NotifyOfPropertyChange();
				NotifyOfPropertyChange(nameof(CanDecreasePriorityCmd));
				NotifyOfPropertyChange(nameof(CanIncreasePriorityCmd));
				NotifyOfPropertyChange(nameof(CanRemoveProfileCmd));
				NotifyOfPropertyChange(nameof(CanMakeDefaultCmd));
				NotifyOfPropertyChange(nameof(CanProfilePropertiesCmd));
				NotifyOfPropertyChange(nameof(CanExportProfileCmd));
			}
		}

		#endregion

		#region [ Methods ]

		/// <summary>
		/// Filters the profiles according to the selected WiFi interface.
		/// </summary>
		private void ApplyFilter(object sender, FilterEventArgs e)
		{
			Profile wifiProfile = e.Item as Profile;
			if (wifiProfile != null && SelectedInterface != null)
			{
				e.Accepted = true;
				if (wifiProfile.Id == SelectedInterface.Id)
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
		/// Retrieves the WiFi profiles and interfaces and updates the View.  
		/// </summary>
		private async Task DownloadProfilesAsync()
		{
			BusyStateManager.EnterBusy();
			BusyStateManager.SetMessage(SeverityType.Info, "Retrieving profiles and interfaces.");

			/* Temporary store selected interface and wifi profile so they can be restored after download. */
			Guid? interfaceId = SelectedInterface?.Id;
			string profileName = SelectedWiFiProfile?.ProfileName;

			/* Download wifi profiles and interfaces. */
			IEnumerable<Profile> profiles = await _mainController.GetWiFiProfilesAsync();
			IEnumerable<Interface> interfaces = await _mainController.GetWiFiInterfacesAsync();

			if (profiles == default(IEnumerable<Profile>) || interfaces == default(IEnumerable<Interface>))
			{
				WiFiProfiles = new ObservableCollection<Profile>(Enumerable.Empty<Profile>());
				WiFiProfilesViewSource.Source = WiFiProfiles;
				Interfaces = new ObservableCollection<Interface>(Enumerable.Empty<Interface>());
			}
			else
			{
				WiFiProfiles = new ObservableCollection<Profile>(profiles);
				WiFiProfilesViewSource.Source = WiFiProfiles;
				Interfaces = new ObservableCollection<Interface>(interfaces);
			}

			/* Restore selected interface and wifi profile. */
			Interface selectedInterface = Interfaces.Where(x => x.Id == interfaceId).FirstOrDefault();
			Profile selectedWifiProfile = WiFiProfiles.Where(x => x.Id == interfaceId &&
			x.ProfileName == profileName).FirstOrDefault();

			SelectedInterface = selectedInterface == default(Interface) ?
				Interfaces.FirstOrDefault() : selectedInterface;
			if (selectedWifiProfile != default(Profile))
			{
				SelectedWiFiProfile = selectedWifiProfile;
			}

			BusyStateManager.SetMessage(SeverityType.None);
			BusyStateManager.ExitBusy();
        }

		/// <summary>
		/// Remove the selected profile.  
		/// </summary>
		private async Task RemoveProfileAsync()
		{
			BusyStateManager.EnterBusy();
			BusyStateManager.SetMessage(SeverityType.Info, "Removing selected profile.");

			/* Temporary store selected interface and WiFi profile so they can be restored after deletion. */
			Guid selectedInterfaceId = SelectedInterface.Id;
			int profilePosition = SelectedWiFiProfile.Position;

			await _mainController.DeleteProfileAsync(SelectedWiFiProfile);
			await DownloadProfilesAsync();

			/* Restore selected interface and position of selected WiFi profile. */
			Profile selectedWifiProfile = WiFiProfiles.Where(x => x.Id == selectedInterfaceId &&
			x.Position == profilePosition).FirstOrDefault();
			if (selectedWifiProfile == default(Profile))
			{
				selectedWifiProfile = WiFiProfiles.Where(x => x.Id ==
				selectedInterfaceId).OrderByDescending(x => x.Position).FirstOrDefault();
			}

			Interface selectedInterface = Interfaces.Where(x => x.Id == selectedInterfaceId).FirstOrDefault();

			SelectedInterface = selectedInterface == default(Interface) ?
				Interfaces.FirstOrDefault() : selectedInterface;
			if (selectedWifiProfile != default(Profile))
			{
				SelectedWiFiProfile = selectedWifiProfile;
			}

			BusyStateManager.SetMessage(SeverityType.None);
			BusyStateManager.ExitBusy();
        }

		/// <summary>
		/// Set the selected profile as default connection profile (position 0).  
		/// </summary>
		private async Task MakeProfileDefaultAsync()
		{
			BusyStateManager.EnterBusy();
			BusyStateManager.SetMessage(SeverityType.Info, 
				"Setting the selected profile as the default connection profile.");

			await _mainController.SetProfileAsDefaultAsync(SelectedWiFiProfile);
			await DownloadProfilesAsync();

			BusyStateManager.SetMessage(SeverityType.None);
			BusyStateManager.ExitBusy();
        }

        /// <summary>
        /// Increase the priority of the selected profile.  
        /// </summary>
        private async Task IncreasePriorityAsync()
		{
			BusyStateManager.EnterBusy();
			BusyStateManager.SetMessage(SeverityType.Info, "Increasing priority of the selected profile.");

			await _mainController.MoveUpProfileAsync(SelectedWiFiProfile);
			await DownloadProfilesAsync();

			BusyStateManager.SetMessage(SeverityType.None);
			BusyStateManager.ExitBusy();
        }

        /// <summary>
        /// Decrease the priority of the selected profile.  
        /// </summary>
        private async Task DecreasePriorityAsync()
		{
			BusyStateManager.EnterBusy();
			BusyStateManager.SetMessage(SeverityType.Info, "Decreasing priority of the selected profile.");

			await _mainController.MoveDownProfileAsync(SelectedWiFiProfile);
			await DownloadProfilesAsync();

			BusyStateManager.SetMessage(SeverityType.None);
			BusyStateManager.ExitBusy();
        }

		/// <summary>
		/// Exporting the selected WiFi XML profile to file. The profile retrieved from
		/// Windows is formatted using _profileProvider.Format before saving. 
		/// </summary>
		private async Task ExportProfileAsync()
		{
			BusyStateManager.EnterBusy();
			BusyStateManager.SetMessage(SeverityType.Info, "Opening SaveFileDialog.");

			await Task.Run(() =>
			{
				SaveFileDialog dlg = new SaveFileDialog();
				dlg.FileName = SelectedWiFiProfile.ProfileName; // Default file name
				dlg.DefaultExt = ".xml"; // Default file extension
				dlg.Filter = "XML file (*.xml) | *.xml"; // Filter files by extension
				dlg.InitialDirectory = _configurationProvider.ExportDir;
				dlg.Title = "Save profile As";

				bool result = dlg.ShowDialog() ?? false;

				if (result)
				{
					BusyStateManager.SetMessage(SeverityType.Info, "Exporting profile.");
					File.WriteAllText(dlg.FileName, _profileService.Format(SelectedWiFiProfile.Xml));
					/* Save last visited directory. */
					_configurationProvider.ExportDir = Path.GetDirectoryName(dlg.FileName);
				}
			});

			BusyStateManager.SetMessage(SeverityType.None);
			BusyStateManager.ExitBusy();
        }

		/// <summary>
		/// Imports WiFi XML profile from file. After validation, the imported profile is added
		/// to the selected WiFi interface.
		/// </summary>
		private async Task ImportProfileAsync()
		{
			bool isValid = false;

			BusyStateManager.EnterBusy();
			BusyStateManager.SetMessage(SeverityType.Info, "Opening OpenFileDialog.");

			OpenFileDialog dlg = new OpenFileDialog();
			dlg.DefaultExt = ".xml"; // Default file extension
			dlg.Filter = "XML file (*.xml) | *.xml"; // Filter files by extension
			dlg.InitialDirectory = _configurationProvider.ImportDir;
			dlg.Title = "Import profile.";
			dlg.Multiselect = false;

			bool result = dlg.ShowDialog() ?? false;
			if (result)
			{
				string profileXml = File.ReadAllText(dlg.FileName);
				/* Validate WiFi XML profile before importing it. */
				Profile profile = new Profile();
				isValid = _profileService.Parse(profileXml, ref profile);

				if (isValid)
				{
					BusyStateManager.SetMessage(SeverityType.Info, "Importing profile.");
					isValid = await _mainController.AddProfileAsync(SelectedInterface, profileXml);
					await DownloadProfilesAsync();
				}

				/* Set dialog initial directory to last visited directory. */
				_configurationProvider.ImportDir = Path.GetDirectoryName(dlg.FileName);
			}

			if (result && !isValid)
			{
				BusyStateManager.SetMessage(SeverityType.Error,
					"Failed to validate the profile, please check the file.");
			}
			else
			{
				BusyStateManager.SetMessage(SeverityType.None);
			}

			BusyStateManager.ExitBusy();
		}

		#endregion

		#region [ Button Commands ]

		/// <summary>
		/// Refreshes the profile and interface list.  
		/// </summary>
		public void RefreshViewCmd()
		{
			DownloadProfilesAsync().FireAndForgetSafeAsync(_errorHandler);
		}

		/// <summary>
		/// Import profile.  
		/// </summary>
		public void ImportProfileCmd()
		{
			ImportProfileAsync().FireAndForgetSafeAsync(_errorHandler);
		}

		/// <summary>
		/// If an interface is selected, enable import command.  
		/// </summary>
		public bool CanImportProfileCmd
		{
			get { return SelectedInterface != null; }
		}

		/// <summary>
		/// Export profile to file.  
		/// </summary>
		public void ExportProfileCmd()
		{
			ExportProfileAsync().FireAndForgetSafeAsync(_errorHandler);
		}

		/// <summary>
		/// If an profile is selected, enable export command.  
		/// </summary>
		public bool CanExportProfileCmd
		{
			get
			{
				bool canExportProfile = false;

				if (SelectedWiFiProfile != null)
				{
					canExportProfile = true;
				}

				return canExportProfile;
			}
		}

		/// <summary>
		/// Remove selected profile.  
		/// </summary>
		public void RemoveProfileCmd()
		{
			RemoveProfileAsync().FireAndForgetSafeAsync(_errorHandler);
		}

		/// <summary>
		/// If selected profile is not connected, enable remove command.  
		/// </summary>
		public bool CanRemoveProfileCmd
		{
			get
			{
				bool canRemove = false;

				if (SelectedWiFiProfile != null &&
					SelectedInterface != null &&
					!SelectedWiFiProfile.IsConnected)
				{
					canRemove = true;
				}

				return canRemove;
			}
		}

		/// <summary>
		/// Increase priority of selected profile.  
		/// </summary>
		public void IncreasePriorityCmd()
		{
			IncreasePriorityAsync().FireAndForgetSafeAsync(_errorHandler);
		}

		/// <summary>
		/// If selected profile is not first in list, enable move up command. 
		/// </summary>
		public bool CanIncreasePriorityCmd
		{
			get
			{
				bool canMoveUp = false;

				if (SelectedWiFiProfile != null &&
					SelectedWiFiProfile.Position != 0)
				{
					canMoveUp = true;
				}

				return canMoveUp;
			}
		}

		/// <summary>
		/// Decrease priority of selected profile.  
		/// </summary>
		public void DecreasePriorityCmd()
		{
			DecreasePriorityAsync().FireAndForgetSafeAsync(_errorHandler);
		}

		/// <summary>
		/// If selected profile is not last in list, enable move down command. 
		/// </summary>
		public bool CanDecreasePriorityCmd
		{
			get
			{
				bool canMoveDown = false;

				if (SelectedWiFiProfile != null)
				{
					canMoveDown = WiFiProfiles.Any(x => x.Id == SelectedWiFiProfile.Id &&
					SelectedWiFiProfile.Position < x.Position);
				}

				return canMoveDown;
			}
		}

		/// <summary>
		/// Make selected profile the default connection profile.  
		/// </summary>
		public void MakeDefaultCmd()
		{
			MakeProfileDefaultAsync().FireAndForgetSafeAsync(_errorHandler);
		}
		/// <summary>
		/// If selected profile is not already default connection profile,
		/// enable MakeDefaultCmd() command. 
		/// </summary>
		public bool CanMakeDefaultCmd
		{
			get
			{
				bool canMakeDefault = false;

				if (SelectedWiFiProfile != null &&
					SelectedWiFiProfile.Position != 0)
				{
					canMakeDefault = true;
				}

				return canMakeDefault;
			}
		}

		/// <summary>
		/// Show properties of selected profile and interface.  
		/// </summary>
		public void ProfilePropertiesCmd()
		{
			try
			{
				_propertiesViewModel.SetProfile(SelectedInterface, SelectedWiFiProfile);
				_windowManager.ShowDialog(_propertiesViewModel);
			}
			catch (Exception ex)
			{
				BusyStateManager.SetMessage(SeverityType.Error, ex.Message);
				BusyStateManager.ClearBusy();
			}
		}

		/// <summary>
		/// If interface and profile selected, enable show properties command.  
		/// </summary>
		public bool CanProfilePropertiesCmd
		{
			get
			{
				bool canShowProperties = false;

				if (SelectedInterface != null && SelectedWiFiProfile != null)
				{
					canShowProperties = true;
				}

				return canShowProperties;
			}
		}

		#endregion
	}
}
