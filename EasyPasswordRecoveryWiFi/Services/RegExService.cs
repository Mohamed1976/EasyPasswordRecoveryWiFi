using System;
using com.mifmif.common.regex.util;
using com.mifmif.common.regex;
using System.Text.RegularExpressions;
using EasyPasswordRecoveryWiFi.Interfaces;

namespace EasyPasswordRecoveryWiFi.Services
{
	public class RegExService : IRegExService
	{
		#region [ Private members ]

		private Generex _generex = null;
		private Iterator _iterator = null;

		#endregion

		#region [ Constructors ]

		public RegExService() { }

		#endregion

		#region [ Methods ]

		/// <summary>
		/// Set regular expression used to generate strings.   
		/// </summary>
		/// <param name="regEx">Regular expression used to generate strings.</param>
		/// <returns>True if regular expression is valid, false otherwise.</returns>
		public bool SetRegEx(string regEx)
		{
			bool isValid = false;

			if (VerifyRegEx(regEx))
			{
				_generex = new Generex(regEx);
				// Using Generex iterator
				_iterator = _generex.iterator();
				isValid = _iterator.hasNext();
			}

			return isValid;
		}

		/// <summary>
		/// Return next string that matches specified regular expression. 
		/// </summary>
		/// <returns>Return generated string that matches specified regular expression.</returns>
		public string GetNext()
		{
			string match = null;

			if (_iterator != null &&
				_iterator.hasNext())
			{
				match = _iterator.next();
			}

			return match;
		}

		/// <summary>
		/// Validates regular expression.   
		/// </summary>
		/// <param name="regEx">Regular expression used to generate strings.</param>
		/// <returns>True if regular expression is valid, false otherwise.</returns>
		private bool VerifyRegEx(string regex)
		{
			bool isValid = false;

			if ((regex != null) && (regex.Trim().Length > 0))
			{
				try
				{
					Regex.Match("", regex);
					isValid = true;
				}
				catch (ArgumentException) { }
			}

			return (isValid);
		}

		#endregion
	}
}
