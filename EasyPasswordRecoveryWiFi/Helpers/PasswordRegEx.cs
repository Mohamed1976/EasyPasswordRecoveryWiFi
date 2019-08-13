using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Interfaces;

namespace EasyPasswordRecoveryWiFi.Helpers
{
	public class PasswordRegEx : Screen
	{
		#region [ Injected instances ]

		private readonly IRegExService _regExService = null;

		#endregion

		#region [ Constructor ]

		public PasswordRegEx(IRegExService regExService, string regEx)
		{
			_regExService = regExService;
			RegEx = regEx;
		}

		#endregion

		#region [ Properties ]

		public string RegEx { get; set; }

		private int count = 0;

		/// <summary>
		/// Returns the number of strings generated using RegEx.
		/// </summary>
		public int Count
		{
			get { return count; }
			set
			{
				if (Set(ref count, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		#endregion

		#region [ Methods ] 

		/// <summary>
		/// The GetFirst method returns the first string that meets the regular expression.
		/// Returns null if no string meets the regular expression. 
		/// </summary>
		/// <remarks>
		/// The GetFirst method rewinds the RegEx generator. 
		/// </remarks>
		public string GetFirst()
		{
			string match = null;

			Count = 0;
			if (_regExService.SetRegEx(RegEx))
			{
				match = _regExService.GetNext();
			}

			if (match != null)
			{
				Count++;
			}

			return match;
		}

		/// <summary>
		/// The GetNext method returns the next string generated using RegEx.
		/// Returns null if no more strings can be generated using the RegEx.
		/// </summary>
		public string GetNext()
		{
			string match = _regExService.GetNext();

			if (match != null)
			{
				Count++;
			}

			return match;
		}

		#endregion
	}
}
