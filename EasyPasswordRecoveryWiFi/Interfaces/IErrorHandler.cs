using System;

namespace EasyPasswordRecoveryWiFi.Interfaces
{
	/// <summary>
	/// Error handler used for exception handling.  
	/// </summary>
	public interface IErrorHandler
	{
		void HandleError(Exception ex);
	}
}
