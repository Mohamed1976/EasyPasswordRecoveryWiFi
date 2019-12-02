using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Helpers
{
    /// <summary>
    /// Class representing whether the application is currently busy.  
    /// Tracks the number of times EnterBusy() and ExitBusy() has been called 
    /// and aggregates this into an effective 'true' or 'false' accordingly.
    /// </summary>
    public class BusyStateManager : PropertyChangedBase, IBusyStateManager
    {
	private readonly object isBusyLock = new object();
	private readonly object messageLock = new object();

	private int _busyCounter;
	private string _message;
	private bool _isBusy;

        public BusyStateManager()
        {
            _busyCounter = 0;
            _isBusy = false;
            _message = string.Empty;
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                _isBusy = value;
                NotifyOfPropertyChange();
            }
        }

        public string Message
        {
            get { return _message; }
            private set
            {
                _message = value;
                NotifyOfPropertyChange();
            }
        }

        public void ClearBusy()
        {
		lock (isBusyLock)
		{
            		_busyCounter = 0;
            		IsBusy = false;
	    	}
        }

        public void EnterBusy()
        {
		lock (isBusyLock)
		{
			_busyCounter++;
			if (_busyCounter == 1)
			{
				IsBusy = true;
			}
		}
        }

        public void ExitBusy()
        {
		lock (isBusyLock)
		{
			if (_busyCounter == 0)
			{
				throw new InvalidOperationException("BusyCounter is already zero.");
			}

			_busyCounter--;
			if (_busyCounter == 0)
			{
				IsBusy = false;
			}
		}
        }

        public void SetMessage(SeverityType severityType, string message = null)
        {
		lock (messageLock)
		{
			if(severityType == SeverityType.None || string.IsNullOrEmpty(message))
			{
				Message = string.Empty;
			}
			else
			{
				Message = string.Format("{0}: {1}", severityType, message);
			}
		}
	}
    }
}
