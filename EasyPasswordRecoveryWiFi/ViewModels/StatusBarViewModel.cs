using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Interfaces;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class StatusBarViewModel : Screen, IShell
	{

        #region [ Constructor ]

		public StatusBarViewModel(IBusyStateManager busyStateManager)
		{
			BusyStateManager = busyStateManager;
			IsEnabled = true;
		}

		#endregion

		#region [ Screen overrides ]

		protected override void OnInitialize()
		{
			base.OnInitialize();
		}

		protected override void OnActivate()
		{
			base.OnActivate();
		}

		protected override void OnDeactivate(bool close)
		{
		    base.OnDeactivate(close);
		}

        #endregion

        #region [ Properties ]

		public IBusyStateManager BusyStateManager { get; }

        private bool _isEnabled;
        /// <summary>
        /// Enable/Disable statusbar.
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (Set(ref _isEnabled, value))
                {
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion
    }
}