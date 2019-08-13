using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Helpers;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Messages;
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
		private readonly IBusyIndicator _busyIndicator = null;
		private readonly IErrorHandler _errorHandler = null;
		private readonly IEventAggregator _eventAggregator = null;

		#endregion

		#region [ Constructor ]

		public AddRegexViewModel(IEventAggregator eventAggregator,
			IBusyIndicator busyIndicator,
			IErrorHandler errorHandler,
			IRegExService regExService,
			StatusBarViewModel statusBarViewModel)
		{
			_eventAggregator = eventAggregator;
			_busyIndicator = busyIndicator;
			_errorHandler = errorHandler;
			_regExService = regExService;
			statusBarViewModel.Name = "RegExStatusBar";
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
			MaxRows = 100;
			RegEx = null;
			RegExMatches?.Clear();
			/* Disable MainStatusBar, so that only one Status Bar is visible to the user. */
			_eventAggregator.PublishOnUIThread(new ActivateMsg(false, "MainStatusBar"));
			_busyIndicator.ApplicationState += HandleBusyEvent;
			base.OnActivate();
		}

		/// <summary>
		/// Cleanup when this view is deactivated.
		/// </summary>
		protected override void OnDeactivate(bool close)
		{
			/* Clear status messages from current Dialog Window. */
			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.None));
			/* Enable MainStatusBar, because current Dialog Window is closed. */
			_eventAggregator.PublishOnUIThread(new ActivateMsg(true, "MainStatusBar"));
			_busyIndicator.ApplicationState -= HandleBusyEvent;
			base.OnDeactivate(close);
		}

		#endregion

		#region [ Properties ]

		private bool isBusy = false;
		/// <summary>
		/// Busy flag used to enable/disable controls. 
		/// </summary>
		public bool IsBusy
		{
			get { return isBusy; }
			set
			{
				if (Set(ref isBusy, value))
				{
					NotifyOfPropertyChange();
					NotifyOfPropertyChange(nameof(CanStartCmd));
					NotifyOfPropertyChange(nameof(CanStopCmd));
					NotifyOfPropertyChange(nameof(CanCancelCmd));
					NotifyOfPropertyChange(nameof(CanSelectCmd));
				}
			}
		}

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
			_busyIndicator.IsBusy = true;
			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info,
				"Generating strings that match the specified RegEx."));

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

			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.None));
			_busyIndicator.IsBusy = false;
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
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info,
					"Please enter a valid regular expression."));
			}
		}

		/// <summary>
		/// If not busy enable start command.  
		/// </summary>
		public bool CanStartCmd
		{
			get { return !IsBusy; }
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
			get { return IsBusy; }
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
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info,
					"Please enter a valid regular expression."));
			}
		}

		/// <summary>
		/// If not busy enable RegEx select command.  
		/// </summary>
		public bool CanSelectCmd
		{
			get { return !IsBusy; }
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
			get { return !IsBusy; }
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
