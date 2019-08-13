using EasyPasswordRecoveryWiFi.Models.Wlan;
using System.Xml;

namespace EasyPasswordRecoveryWiFi.Interfaces
{
	/// <summary>
	/// Profile factory used to create, validate and format connection profiles.  
	/// </summary>
	public interface IProfileService
	{
		string CreateProfileXml(AccessPoint accessPoint, string password);
		bool Parse(string profileXml, ref Profile profile);
		string Format(string fileXml, XmlWriterSettings settings = null);
	}
}
