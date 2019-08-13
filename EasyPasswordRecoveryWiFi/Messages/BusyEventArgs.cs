using System;

namespace EasyPasswordRecoveryWiFi.Messages
{
	public class BusyEventArgs : EventArgs
	{
		public BusyEventArgs(bool isBusy)
		{
			IsBusy = isBusy;
		}

		public bool IsBusy { get; }
	}
}
