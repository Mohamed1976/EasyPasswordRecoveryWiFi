using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Messages;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class StatusBarViewModel : Screen, IShell, IHandle<StatusMsg>, IHandle<ActivateMsg>
	{
		#region [ Injected instances ]

		private readonly IEventAggregator _eventAggregator = null;
		private readonly IBusyIndicator _busyIndicator = null;

		#endregion

		#region [ Constructor ]

		public StatusBarViewModel(IEventAggregator eventAggregator, IBusyIndicator busyIndicator)
		{
			_eventAggregator = eventAggregator;
			_busyIndicator = busyIndicator;
		}

		#endregion

		#region [ Screen overrides ]

		protected override void OnInitialize()
		{
			base.OnInitialize();
		}

		protected override void OnActivate()
		{
			_busyIndicator.ApplicationState += HandleBusyEvent;
			_eventAggregator.Subscribe(this);
			base.OnActivate();
		}

		protected override void OnDeactivate(bool close)
		{
			_busyIndicator.ApplicationState -= HandleBusyEvent;
			_eventAggregator.Unsubscribe(this);
			base.OnDeactivate(close);
		}

		#endregion

		#region [ Properties ]

		private bool isBusy = false;
		/// <summary>
		/// Busy flag used to show/hide progress bar.
		/// </summary>
		public bool IsBusy
		{
			get { return isBusy; }
			set
			{
				if (Set(ref isBusy, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private bool isEnabled = true;
		/// <summary>
		/// Enable/Disable statusbar.
		/// </summary>
		public bool IsEnabled
		{
			get { return isEnabled; }
			set
			{
				if (Set(ref isEnabled, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		/// <summary>
		/// Name of the statusbar used to Enable/Disable a specific statusbar.
		/// </summary>
		public string Name { get; set; }

		private string statusMessage;
		/// <summary>
		/// Message displayed in the statusbar view.
		/// </summary>
		public string StatusMessage
		{
			get { return statusMessage; }
			set
			{
				if (Set(ref statusMessage, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		#endregion

		#region [ Event handlers]

		/// <summary>
		/// Event used to set and clear the message property.
		/// </summary>
		public void Handle(StatusMsg msg)
		{
			if (msg.Severity == SeverityType.Info ||
				msg.Severity == SeverityType.Warning ||
				msg.Severity == SeverityType.Error)
			{
				StatusMessage = string.Format("{0}: {1}", msg.Severity, msg.Message);
			}
			else if (msg.Severity == SeverityType.None)
			{
				StatusMessage = string.Empty;
			}
		}

		/// <summary>
		/// Event used to set and clear the enable property.
		/// </summary>
		public void Handle(ActivateMsg msg)
		{
			if (msg.TargetName == Name)
			{
				IsEnabled = msg.IsEnabled;
			}
		}

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
