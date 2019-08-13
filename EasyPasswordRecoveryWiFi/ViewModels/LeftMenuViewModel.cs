using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Messages;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class LeftMenuViewModel : Screen, IShell
	{
		#region [ Injected instances ]

		private readonly IBusyIndicator _busyIndicator = null;

		#endregion

		#region [ Constructor ]

		public LeftMenuViewModel(IBusyIndicator busyIndicator)
		{
			_busyIndicator = busyIndicator;
			_busyIndicator.ApplicationState += HandleBusyEvent;
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
				}
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
