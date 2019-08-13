using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Messages
{
	/// <summary>
	/// Message to enable or disable Status Bar in order to avoid that multiple Status Bars are active.
	/// </summary>
	public class ActivateMsg
	{
		public ActivateMsg(bool isEnabled, string targetName)
		{
			IsEnabled = isEnabled;
			TargetName = targetName;
		}

		/// <summary>
		/// Enable or disable Status Bar.
		/// </summary>
		public bool IsEnabled { get; }

		/// <summary>
		/// Target name, target control the message is intended to.
		/// </summary>
		public string TargetName { get; }
	}
}
