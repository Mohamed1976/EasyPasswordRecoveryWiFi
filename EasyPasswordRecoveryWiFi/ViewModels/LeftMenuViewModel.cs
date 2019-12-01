using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Interfaces;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class LeftMenuViewModel : Screen, IShell
	{
		#region [ Constructor ]

		public LeftMenuViewModel(IBusyStateManager busyStateManager)
		{
			BusyStateManager = busyStateManager;
		}

        #endregion

        #region [ Properties ]

		public IBusyStateManager BusyStateManager { get; }

		#endregion
	}
}
