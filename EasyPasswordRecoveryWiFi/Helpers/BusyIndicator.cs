using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Helpers
{
	/// <summary>
	/// Class representing whether the application is currently busy.  
	/// Tracks the number of times it has been set to 'true' and 'false' 
	/// and aggregates this into an effective 'true' or 'false' accordingly.
	/// </summary>
	public class BusyIndicator : IBusyIndicator
	{
		public BusyIndicator()
		{
		}

		/// <summary>
		/// Number of times the <see cref="IsBusy"/> property was set to true.
		/// </summary>
		public int BusyCounter { get; private set; } = 0;

		private bool isBusy = false;

		/// <summary>
		/// Sets a new value for this property, if the new value differs from the old value, 
		/// a PropertyChanged event will be fired.
		/// </summary>
		public bool IsBusy
		{
			get
			{
				return isBusy;
			}
			set
			{
				if (value)
				{
					BusyCounter++;
					if (BusyCounter == 1)
					{
						isBusy = value;
						OnApplicationStateChanged(new BusyEventArgs(value));
					}
				}
				else
				{
					BusyCounter--;
					if (BusyCounter == 0)
					{
						isBusy = value;
						OnApplicationStateChanged(new BusyEventArgs(value));
					}
				}
			}
		}

		/// <summary>
		/// Force the busy state to false.
		/// </summary>
		public void ResetState()
		{
			BusyCounter = 0;
			isBusy = false;
			OnApplicationStateChanged(new BusyEventArgs(false));
		}

		public event EventHandler<BusyEventArgs> ApplicationState;
		/// <summary>
		/// Notify subscribers that busy state has changed.
		/// </summary>
		protected virtual void OnApplicationStateChanged(BusyEventArgs e)
		{
			EventHandler<BusyEventArgs> handler = ApplicationState;
			handler?.Invoke(this, e);
		}
	}
}
