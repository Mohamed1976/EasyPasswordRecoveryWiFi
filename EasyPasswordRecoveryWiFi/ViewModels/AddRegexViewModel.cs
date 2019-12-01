using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Helpers;
using EasyPasswordRecoveryWiFi.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class AddRegexViewModel : Screen, IShell
	{
		#region [ Injected instances ]

		private readonly IRegExService _regExService = null;
		private readonly IErrorHandler _errorHandler = null;
		private readonly IEventAggregator _eventAggregator = null;

		#endregion

		#region [ Constructor ]

		public AddRegexViewModel(IEventAggregator eventAggregator,
			IErrorHandler errorHandler,
			IRegExService regExService,
			IBusyStateManager busyStateManager,
			StatusBarViewModel statusBarViewModel)
		{
			_eventAggregator = eventAggregator;
			_errorHandler = errorHandler;
			_regExService = regExService;
			BusyStateManager = busyStateManager;
            StatusBarBottom = statusBarViewModel;
			StatusBarBottom.ConductWith(this);
		}

		#endregion

		#region [ Screen overrides ]

		protected override void OnInitialize()
		{
			base.OnInitialize();
		}

		/// <summary>
		/// Initializations when this view is activated.
		/// </summary>
		protected override void OnActivate()
		{
			/* When IsBusy is changed, CanCancelCmd, CanStopCmd, CanSelectCmd and CanStartCmd are notified. */
			BusyStateManager.PropertyChanged += BusyStateManagerPropertyChanged;

            MaxRows = 100;
			RegEx = null;
			RegExMatches?.Clear();
			base.OnActivate();
		}

		/// <summary>
		/// Cleanup when this view is deactivated.
		/// </summary>
		protected override void OnDeactivate(bool close)
		{
			BusyStateManager.PropertyChanged -= BusyStateManagerPropertyChanged;
			/* Clear status messages from current Dialog Window. */
			BusyStateManager.SetMessage(SeverityType.None);
			base.OnDeactivate(close);
		}

        #endregion

        #region [ Properties ]

		public IBusyStateManager BusyStateManager { get; }

		/// <summary>
		/// Status Bar to display info/errors. 
		/// </summary>
		public IShell StatusBarBottom { get; }

		private int maxRows = 100;
		/// <summary>
		/// Maximum number of RegEx matches that will be displayed. 
		/// </summary>
		public int MaxRows
		{
			get { return maxRows; }
			set
			{
				if (Set(ref maxRows, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private string regEx = null;
		/// <summary>
		/// Regular expression used to generate passwords. 
		/// </summary>
		public string RegEx
		{
			get { return regEx; }
			set
			{
				if (Set(ref regEx, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private ObservableCollection<string> regExMatches = null;
		/// <summary>
		/// Passwords generated using the specified regular expression. 
		/// </summary>
		public ObservableCollection<string> RegExMatches
		{
			get { return regExMatches; }
			set
			{
				if (Set(ref regExMatches, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		#endregion

		#region [ Methods ]

		private CancellationTokenSource cts = null;
		/// <summary>
		/// Passwords generator using the specified regular expression. 
		/// </summary>
		/// <param name="token">CancellationToken to stop string generation.</param>
		private async Task GenerateStrings(CancellationToken token)
		{
			List<string> matches = new List<string>();
			BusyStateManager.EnterBusy();
			BusyStateManager.SetMessage(SeverityType.Info, "Generating strings that match the specified RegEx.");

			await Task.Run(() =>
			{
				int count = 0;
				string match = null;

				/* Set regular expression and add matches to collection. */
				if (_regExService.SetRegEx(RegEx))
				{
					while (!token.IsCancellationRequested &&
					count++ < MaxRows &&
					(match = _regExService.GetNext()) != null)
					{
						matches.Add(match);
					}
				}
			});

			RegExMatches = new ObservableCollection<string>(matches);

			BusyStateManager.SetMessage(SeverityType.None);
			BusyStateManager.ExitBusy();
		}

		#endregion

		#region [ Command handlers ]

		/// <summary>
		/// Starts generating passwords using specified RegEx.
		/// </summary>
		public void StartCmd()
		{
			if (_regExService.SetRegEx(RegEx))
			{
				cts = new CancellationTokenSource();
				GenerateStrings(cts.Token).FireAndForgetSafeAsync(_errorHandler);
			}
			else
			{
				BusyStateManager.SetMessage(SeverityType.Info, "Please enter a valid regular expression.");
			}
		}

		/// <summary>
		/// If not busy enable start command.  
		/// </summary>
		public bool CanStartCmd
		{
			get { return !BusyStateManager.IsBusy; }
		}

		/// <summary>
		/// Stop password generating process using cancellationtoken.  
		/// </summary>
		public void StopCmd()
		{
			cts?.Cancel();
		}

		/// <summary>
		/// If busy enable stop command.  
		/// </summary>
		public bool CanStopCmd
		{
			get { return BusyStateManager.IsBusy; }
		}

		/// <summary>
		/// Select specified RegEx and return true to the parent view.  
		/// </summary>
		public void SelectCmd()
		{
			if (_regExService.SetRegEx(RegEx))
			{
				TryClose(true);
			}
			else
			{
				BusyStateManager.SetMessage(SeverityType.Info, "Please enter a valid regular expression.");
			}
		}

		/// <summary>
		/// If not busy enable RegEx select command.  
		/// </summary>
		public bool CanSelectCmd
		{
			get { return !BusyStateManager.IsBusy; }
		}

		/// <summary>
		/// Cancel RegEx selection and return false to the parent view.  
		/// </summary>
		public void CancelCmd()
		{
			TryClose(false);
		}

		/// <summary>
		/// If not busy enable cancel command.  
		/// </summary>
		public bool CanCancelCmd
		{
			get { return !BusyStateManager.IsBusy; }
		}

		#endregion

		#region [ Event Handlers ]

		/* When IsBusy is changed, CanCancelCmd, CanStopCmd, CanSelectCmd and CanStartCmd are notified. */
		private void BusyStateManagerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(BusyStateManager.IsBusy))
			{
				NotifyOfPropertyChange(nameof(CanStartCmd));
				NotifyOfPropertyChange(nameof(CanStopCmd));
				NotifyOfPropertyChange(nameof(CanCancelCmd));
				NotifyOfPropertyChange(nameof(CanSelectCmd));
			}
		}

		#endregion
    }
}
