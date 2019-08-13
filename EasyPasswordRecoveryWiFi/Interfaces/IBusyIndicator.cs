using EasyPasswordRecoveryWiFi.Messages;
using System;

namespace EasyPasswordRecoveryWiFi.Interfaces
{
	/// <summary>
	/// BusyIndicator keeps track of the overall busy state of the application. 
	/// </summary>
	public interface IBusyIndicator
	{
		/// <summary>
		/// Notify subscribers that busy state has changed.
		/// </summary>
		event EventHandler<BusyEventArgs> ApplicationState;

		/// <summary>
		/// Tracks the number of times it has been set to 'true' and 'false'. 
		/// </summary>
		int BusyCounter { get; }

		/// <summary>
		/// The overall busy state of the application.
		/// </summary>
		bool IsBusy
		{
			get;
			set;
		}

		/// <summary>
		/// Force the busy state to false.
		/// </summary>
		void ResetState();
	}
}
