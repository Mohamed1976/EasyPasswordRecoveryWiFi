using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Interfaces;
using System;

namespace EasyPasswordRecoveryWiFi.Helpers
{
	public class ErrorHandler : IErrorHandler
	{
		public ErrorHandler(IBusyStateManager busyStateManager)
		{
			BusyStateManager = busyStateManager;
		}

		public IBusyStateManager BusyStateManager { get; }

        /// <summary>
        /// Error is handled by displaying it in the active status bar and resetting the application busy state.
        /// </summary>
		public void HandleError(Exception ex)
		{
			BusyStateManager.SetMessage(SeverityType.Error, ex.Message);
			BusyStateManager.ClearBusy();
		}
	}
}
