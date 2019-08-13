using EasyPasswordRecoveryWiFi.Models.Wlan;

namespace EasyPasswordRecoveryWiFi.Interfaces
{
	/// <summary>
	/// Profile factory used to create connection profiles.  
	/// </summary>
	public interface IProfileFactory
	{
		string CreateProfileXml(AccessPoint accessPoint, string password);
	}
}
