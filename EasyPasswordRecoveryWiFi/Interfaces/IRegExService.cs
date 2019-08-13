using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Interfaces
{
	/// <summary>
	/// RegEx service, generate strings that match the specified regular expression.  
	/// </summary>
	public interface IRegExService
	{
		bool SetRegEx(string regEx);
		string GetNext();
	}
}
