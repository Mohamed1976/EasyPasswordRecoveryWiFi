using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Messages;
using System;

namespace EasyPasswordRecoveryWiFi.Helpers
{
	public class ErrorHandler : IErrorHandler
	{
		#region [ Injected instances ]

		private readonly IEventAggregator _eventAggregator = null;
		private readonly IBusyIndicator _busyIndicator = null;

		#endregion

		public ErrorHandler(IEventAggregator eventAggregator, IBusyIndicator busyIndicator)
		{
			_eventAggregator = eventAggregator;
			_busyIndicator = busyIndicator;
		}

		/// <summary>
		/// Error is handled by displaying it in the active status bar and resetting the application busy state.
		/// </summary>
		public void HandleError(Exception ex)
		{
			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Error, ex.Message));
			_busyIndicator.ResetState();
		}
	}
}
