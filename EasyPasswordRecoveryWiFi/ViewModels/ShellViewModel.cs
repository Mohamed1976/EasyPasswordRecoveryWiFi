using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Interfaces;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell
	{
		#region [ Injected instances ]

		private readonly IEventAggregator _eventAggregator = null;
		private readonly MainViewModel _mainViewModel = null;
		private readonly SettingsViewModel _settingsViewModel = null;
		private readonly AboutViewModel _aboutViewModel = null;
		private readonly ProfileManagerViewModel _profileManagerViewModel = null;

		#endregion

		#region [ Constructor ]

		public ShellViewModel(IEventAggregator eventAggregator,
			MainViewModel mainViewModel,
			ProfileManagerViewModel profileManagerViewModel,
			SettingsViewModel settingsViewModel,
			AboutViewModel aboutViewModel,
			LeftMenuViewModel leftMenuViewModel,
			HeaderMenuViewModel headerMenuViewModel,
			StatusBarViewModel statusBarViewModel)
		{

			_eventAggregator = eventAggregator;
			_mainViewModel = mainViewModel;
			_settingsViewModel = settingsViewModel;
			_aboutViewModel = aboutViewModel;
			_profileManagerViewModel = profileManagerViewModel;
			LeftMenu = leftMenuViewModel;
			HeaderMenu = headerMenuViewModel;
			StatusBarBottom = statusBarViewModel;
			StatusBarBottom.ConductWith(this);
		}

		#endregion

		#region [ Screen overrides ]

		/// <summary>
		/// Set the home view as the default startup view.
		/// </summary>
		protected override void OnInitialize()
		{
			ActivateItem(_mainViewModel);
			base.OnInitialize();
		}

		protected override void OnActivate()
		{
			_eventAggregator.Subscribe(this);
			base.OnActivate();
		}

		protected override void OnDeactivate(bool close)
		{
			_eventAggregator.Unsubscribe(this);
			base.OnDeactivate(close);
		}

		#endregion

		#region [ Properties ]

		/// <summary>
		/// Left menu view displayed on left of the application window.
		/// </summary>
		public IShell LeftMenu { get; }

		/// <summary>
		/// Header view displayed on top of the application window.
		/// </summary>
		public IShell HeaderMenu { get; }

		/// <summary>
		/// StatusBar view displayed on bottom of the application window.
		/// </summary>
		public IShell StatusBarBottom { get; }

		/// <summary>
		/// Set the home view as the main view.
		/// </summary>
		public void HomeViewCmd()
		{
			ActivateItem(_mainViewModel);
		}

		#endregion

		#region [ Command handlers ]

		/// <summary>
		/// Set the profile manager view as the main view.
		/// </summary>
		public void WifiProfileManagerViewCmd()
		{
			ActivateItem(_profileManagerViewModel);
		}

		/// <summary>
		/// Set the settings view as the main view.
		/// </summary>
		public void SettingsViewCmd()
		{
			ActivateItem(_settingsViewModel);
		}

		/// <summary>
		/// Set the about view as the main view.
		/// </summary>
		public void AboutViewCmd()
		{
			ActivateItem(_aboutViewModel);
		}

		#endregion
	}
}
