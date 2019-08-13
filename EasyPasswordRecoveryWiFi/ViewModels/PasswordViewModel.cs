using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Helpers;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Models.Wlan;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class PasswordViewModel : Screen, IShell
	{
		#region [ Screen overrides ]

		protected override void OnInitialize()
		{
			base.OnInitialize();
		}

		/// <summary>
		/// Show access point and interfaces when view is activated. 
		/// </summary>
		protected override void OnActivate()
		{
			HeaderMessage = null;
			ErrorMessage = null;
			Password = null;

			if (SelectedAccessPoint != null)
			{
				string secured = SelectedAccessPoint.IsSecurityEnabled ? "Secured" : "Open";
				HeaderMessage = $"Please enter password for {SelectedAccessPoint.Ssid} ({secured}):";
			}

			base.OnActivate();
		}

		protected override void OnDeactivate(bool close)
		{
			base.OnDeactivate(close);
		}

		#endregion

		#region [ Properties ]

		/// <summary>
		/// Access point for which the password is entered.
		/// </summary>
		public AccessPoint SelectedAccessPoint { get; set; }

		private string password;
		/// <summary>
		/// Password entered by the user.
		/// </summary>
		public string Password
		{
			get { return password; }
			set
			{
				if (Set(ref password, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private string headerMessage;
		/// <summary>
		/// Header displaying ssid of access point.
		/// </summary>
		public string HeaderMessage
		{
			get { return headerMessage; }
			set
			{
				if (Set(ref headerMessage, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private string errorMessage;
		/// <summary>
		/// Error message, reason why password is invalid.
		/// </summary>
		public string ErrorMessage
		{
			get { return errorMessage; }
			set
			{
				if (Set(ref errorMessage, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		#endregion

		#region [ Command handlers ]

		/// <summary>
		/// Cancel password entry, view is closed and false is returned to parent view. 
		/// </summary>
		public void CancelCmd()
		{
			TryClose(false);
		}

		/// <summary>
		/// Confirm password entry. The entered password is validated if it confirms to wifi password rules.
		/// If password is valid, the view is closed and true is returned to parent view.
		/// If password is invalid, an error message is shown to the user. 
		/// </summary>
		public void OkCmd()
		{
			if (SelectedAccessPoint == null)
			{
				ErrorMessage = "No access point specified.";
			}
			else
			{
				/* Validate the entered password.  */
				string errorMsg = string.Empty;
				bool isValid = PasswordHelper.IsValid(Password, SelectedAccessPoint.Encryption, ref errorMsg);
				if (isValid)
				{
					TryClose(true);
				}
				else
				{
					ErrorMessage = errorMsg;
				}
			}
		}

		#endregion
	}
}
