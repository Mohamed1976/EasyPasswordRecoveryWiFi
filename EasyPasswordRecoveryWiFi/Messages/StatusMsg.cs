using EasyPasswordRecoveryWiFi.Common;

namespace EasyPasswordRecoveryWiFi.Messages
{
	public class StatusMsg
	{
		#region[ Constructor ]

		public StatusMsg(SeverityType severity, string message = null)
		{
			Severity = severity;
			Message = message;
		}

		#endregion

		#region[ Properties ]

		/// <summary>
		/// Describes the severity of the <see cref="Message"/> sent.
		/// </summary>
		public SeverityType Severity { get; }

		/// <summary>
		/// Content of the Message sent.
		/// </summary>
		public string Message { get; }

		#endregion
	}
}
